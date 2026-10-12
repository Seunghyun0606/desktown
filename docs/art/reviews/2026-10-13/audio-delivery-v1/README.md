# Approved audio delivery

The user approved the eight sound assets, scene mixes, project usage and runtime connection on 2026-10-13. [verification.json](verification.json) records production paths, unchanged candidate hashes, native playback checks and the final package identity.

All eight production Ogg files exactly match the approved [listening review](../../2026-10-12/audio-v1/review.html). Its authoring/measurement JSON and README describe the pre-approval review stage; the delivered game now has sound. The four offline mixes are references, not recordings of the runtime.

The shipped presentation uses two Town loops and up to four reusable voices. It respects saved Audio=false, visible action frames, native minimization, overlays, system pause and surface hiding. Ordinary quit detaches audio and drains stopped playbacks before teardown. Completion/discovery is heard only when viewing the reveal, with no queued catch-up.

Native tests used the Dummy driver and certify state/timing, not physical speaker output or a full Windows input/DPI release gate. The final build is an unsigned playable MVP. See [delivery checks](../../../../qa/QA_EXECUTION_2026-10-13_AUDIO_MVP.md) for the exact code and package revisions and launch instructions.
