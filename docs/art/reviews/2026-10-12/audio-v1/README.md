# Quiet sound review v1

Open [review.html](review.html) locally and press play. No autoplay or CDN. Only one track plays at once; hiding the page stops all tracks. The volume control affects this review only.

Eight original procedural candidates plus four offline mix examples. These are NOT game recordings, NOT assigned production assets, and do not change the silent MVP. They contain no external samples, recordings or musical works. Delivery and usage approval are pending; no third-party sample license is claimed.

Recipe: `scripts/assets/prepare_audio_review.py`, deterministic seed 121026. Install numpy==2.5.4 and soundfile==0.14.0 in a local environment, then run the recipe with a new output directory. Vorbis container serials need not be byte-identical between encodes. Decoded samples and stored hashes are authoritative.

Technical checks decode every delivered file: Vorbis, 44.1kHz, exact duration, mono one-shots/stereo loops, finite samples, ample sample-peak headroom, quiet one-shot endpoints. A loop wrap step must be no greater than its ordinary 99th-percentile adjacent sample step. This bound catches discontinuities but does not prove perceptually seamless looping. No LUFS or certified intersample peak measurement is claimed. Listen through headphones/speakers to decide timbre, repetition and relative level.

Proposed runtime mix: Town ambience at unity, fire 0.35, bird 0.65, footsteps 0.7; Companion hammer 0.75; reward/discovery cue 0.9. Event spacing is illustrative. Effects must follow actual visible actions. Hidden surfaces stop presentation sound; no catch-up playback. Completion/discovery plays when its screen is viewed, once per reveal. Existing Audio=false is respected; Audio=true will become audible only after approval.
