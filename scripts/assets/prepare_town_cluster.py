"""Prepare review-only House/path/vegetation candidates from AI images.

Requires Pillow 12.3.0. Does not assign catalog entries. Refuses output reuse.
"""
import argparse
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, PngImagePlugin, __version__


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def binary(image):
    result = Image.new("RGBA", image.size)
    result.putdata([(*p[:3], 255) if p[3] >= 128 else (0,0,0,0)
                    for p in image.get_flattened_data()])
    return result


def fit(image, cell, maximum):
    bounds = image.getchannel("A").point(lambda a:255 if a>=128 else 0).getbbox()
    if not bounds:
        raise ValueError("Transparent source")
    cropped = image.crop(bounds)
    factor = min(maximum[0]/cropped.width, maximum[1]/cropped.height)
    size = (round(cropped.width*factor), round(cropped.height*factor))
    small = binary(cropped.resize(size, Image.Resampling.NEAREST))
    result = Image.new("RGBA", cell)
    result.paste(small, (cell[0]//2-size[0]//2, cell[1]-size[1]))
    return result, {"bounds":bounds, "size":size, "cell":cell}


def repeat(tile):
    result = Image.new("RGBA", (96,96))
    for y in range(3):
        for x in range(3):
            result.paste(tile, (32*x,32*y))
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source_dir",type=Path)
    parser.add_argument("workshop",type=Path)
    parser.add_argument("mina",type=Path)
    parser.add_argument("output",type=Path)
    args = parser.parse_args()
    names = ("house","grass","path","bush")
    paths = {name:args.source_dir/f"{name}.png" for name in names}
    paths.update(workshop=args.workshop, mina=args.mina)
    hashes = {name:digest(path) for name,path in paths.items()}
    images = {name:Image.open(path).convert("RGBA") for name,path in paths.items()}
    house,house_fit = fit(images["house"], (128,128), (112,104))
    bush,bush_fit = fit(images["bush"], (32,32), (28,24))
    grass_source = images["grass"]
    if grass_source.getchannel("A").getextrema() != (255,255):
        raise ValueError("Grass must be opaque; do not invent a background")
    edge = min(grass_source.size)
    left,top = (grass_source.width-edge)//2,(grass_source.height-edge)//2
    grass = grass_source.crop((left,top,left+edge,top+edge)).resize((32,32), Image.Resampling.NEAREST)
    # Keep AI texture colors but reduce competing one-pixel texture noise.
    # Quantization follows this deterministic color/cluster cleanup.
    clustered = grass.resize((16,16),Image.Resampling.NEAREST)
    grass_pixels = list(clustered.get_flattened_data())
    base = tuple(round(sum(p[c] for p in grass_pixels)/len(grass_pixels)) for c in range(3))
    clustered.putdata([(*(round(base[c]+(p[c]-base[c])*.35) for c in range(3)),255)
                       for p in grass_pixels])
    grass = clustered.resize((32,32),Image.Resampling.NEAREST)
    path_source = images["path"]
    path_bounds = path_source.getchannel("A").point(lambda a:255 if a>=128 else 0).getbbox()
    if not path_bounds:
        raise ValueError("Blank path")
    band = binary(path_source.crop(path_bounds).resize((32,16), Image.Resampling.NEAREST))
    path = Image.new("RGBA", (32,32))
    path.paste(band, (0,8))
    frames = {"house":house,"grass":grass,"path":path,"bush":bush}
    visible = [p[:3] for im in frames.values() for p in im.get_flattened_data() if p[3]]
    sample = Image.new("RGB", (len(visible),1))
    sample.putdata(visible)
    colors = iter(sample.quantize(32, Image.Quantize.MEDIANCUT,
                                  dither=Image.Dither.NONE).convert("RGB").get_flattened_data())
    for im in frames.values():
        im.putdata([(*next(colors),255) if p[3] else (0,0,0,0) for p in im.get_flattened_data()])
    # A one-pixel periodic edge band, copied from the opposite edge,
    # locks boundary colors. Visual repetition still requires human review.
    grass = frames["grass"]
    grass.paste(grass.crop((0,0,1,32)),(31,0))
    grass.paste(grass.crop((0,0,32,1)),(0,31))
    path = frames["path"]
    path.paste(path.crop((0,0,1,32)),(31,0))
    assert list(grass.crop((0,0,1,32)).get_flattened_data()) == list(grass.crop((31,0,32,32)).get_flattened_data())
    assert list(grass.crop((0,0,32,1)).get_flattened_data()) == list(grass.crop((0,31,32,32)).get_flattened_data())
    assert list(path.crop((0,0,1,32)).get_flattened_data()) == list(path.crop((31,0,32,32)).get_flattened_data())
    assert all(path.getpixel((x,y))[3] == 0 for x in range(32) for y in list(range(8))+list(range(24,32)))
    palette = sorted({p[:3] for im in frames.values() for p in im.get_flattened_data() if p[3]})
    assert len(palette)<=32
    for name,im in frames.items():
        assert im.size == ((128,128) if name=="house" else (32,32))
        assert all(p[3] in (0,255) and (p[3] or p[:3]==(0,0,0)) for p in im.get_flattened_data())
    assert frames["house"].getchannel("A").getbbox()[3] == 128
    assert frames["bush"].getchannel("A").getbbox()[3] == 32
    assert images["workshop"].size == (160,128) and images["mina"].size == (48,48)
    args.output.mkdir(parents=True,exist_ok=False)
    srgb = PngImagePlugin.PngInfo()
    srgb.add(b"sRGB",b"\x00")
    outputs = []

    def save(im,name):
        target = args.output/name
        im.save(target,pnginfo=srgb)
        with Image.open(target) as checked:
            assert checked.mode==im.mode and checked.size==im.size and checked.tobytes()==im.tobytes()
            assert checked.info.get("srgb")==0
        outputs.append({"file":name,"size":im.size,"sha256":digest(target)})

    for name,im in frames.items():
        for scale in (1,2,3):
            enlarged = im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST)
            assert all(enlarged.getpixel((x,y))==im.getpixel((x//scale,y//scale))
                       for y in range(enlarged.height) for x in range(enlarged.width))
            save(enlarged,f"{name}-{scale}x.png")
    for name in ("grass","path"):
        save(repeat(frames[name]),f"{name}-repeat-3x3.png")
    # A deliberately composed style study on a 32px grid, not runtime Town.
    cluster = Image.new("RGBA",(384,288))
    for y in range(0,288,32):
        for x in range(0,384,32):
            cluster.paste(grass,(x,y))
    for x in range(0,384,32):
        cluster.alpha_composite(path,(x,192))
    for x in (96,256):
        vertical = path.transpose(Image.Transpose.ROTATE_90)
        for y in range(160,224,32):
            cluster.alpha_composite(vertical,(x,y))
    cluster.alpha_composite(frames["house"],(48,32))
    cluster.alpha_composite(images["workshop"],(192,32))
    for position in ((16,144),(160,144),(336,144),(32,224),(304,224)):
        cluster.alpha_composite(frames["bush"],position)
    cluster.alpha_composite(images["mina"],(168,174))
    for scale in (1,2,3):
        save(cluster.resize((384*scale,288*scale),Image.Resampling.NEAREST),f"cluster-{scale}x.png")
    gray = cluster.convert("L").convert("RGBA")
    gray.putalpha(cluster.getchannel("A"))
    save(gray,"cluster-gray-1x.png")
    board = Image.new("RGB",(800,344),"#f3efe7")
    draw = ImageDraw.Draw(board)
    draw.text((16,8),"Town tile style study | 32px grid | House + path + vegetation",fill="#342c27")
    draw.text((16,28),"Color 1x",fill="#342c27")
    draw.text((416,28),"Grayscale 1x",fill="#342c27")
    board.paste(cluster,(8,48),cluster)
    board.paste(gray,(408,48),gray)
    save(board,"comparison-board.png")
    assert all(digest(paths[name])==hashes[name] for name in paths)
    report = {
        "status":"style_candidate_pending_human_approval", "tool":"Python/Pillow", "pillowVersion":__version__,
        "sources":{name:{"file":path.as_posix(),"sha256":hashes[name],"size":images[name].size} for name,path in paths.items()},
        "houseFit":house_fit,"bushFit":bush_fit,"grassCenterCrop":[left,top,left+edge,top+edge],
        "grassCleanup":{"clusters":[2,2],"baseRGB":base,"contrastFactor":.35},
        "pathBounds":path_bounds,"pathBand":[32,16],"pathBandOrigin":[0,8],
        "edgeCleanup":"Grass last column/row copy first column/row; path last column copies first.",
        "pivot":{"house":[64,128],"bush":[16,32]},
        "logicalCellInTiles":{"house":[4,4],"bush":[1,1]},
        "reviewLayout":{"canvas":[384,288],"groundGrid":32,"houseOrigin":[48,32],
                        "workshopOrigin":[192,32],"minaOrigin":[168,174],
                        "pathRow":192,"branchColumns":[96,256]},
        "paletteRGB":palette,"opaqueColorCount":len(palette),"resampling":"nearest","alphaThreshold":128,
        "checks":{"dimensions":True,"binaryAlpha":True,"grassOpaque":True,"transparentRGBZero":True,
                  "sharedPaletteLimit32":True,"nearestEnlargement":True,"srgb":True,"sourcePreserved":True,
                  "grassOppositeEdgesEqual":True,"pathHorizontalEdgesEqual":True,"repeat3x3":True},
        "limits":"Edge equality does not prove seamless perception. One representative grass/path/bush only; "
                 "no final variant sheets, autotile topology, runtime catalog assignment or Windows QA. "
                 "Mina/Workshop retained their approved pixels and separate candidate palettes.",
        "files":outputs,
    }
    (args.output/"preparation.json").write_text(json.dumps(report,indent=2)+"\n",encoding="utf-8")
    print(json.dumps({"output":str(args.output),"colors":len(palette),"checks":report["checks"]}))


if __name__ == "__main__":
    main()
