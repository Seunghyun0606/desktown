# DeskTown Windows 11 laptop QA kit

The kit contains the versioned unsigned app ZIP, its SHA-256 checksum and file
manifest, three isolated demo saves, and PowerShell helpers. No editor or SDK is
needed. Keep the ZIP and manifest together. The build SHA in the manifest is the
issue-report identifier.

1. Extract the GitHub Actions artifact to an empty folder. In PowerShell in that
   folder run `./preflight.ps1 -RunSmoke`. It verifies the ZIP and every extracted
   file, then creates, recovers, and reloads a disposable save in three separate
   headless processes. On success the executable is `./app/DeskTown.exe`.
2. Run `./collect-evidence.ps1 -Scaling '100%'` and fill the created
   `qa-results.md` during the interactive checks. Set scaling to the actual
   Windows display value; monitor resolution/origin and Windows version are
   captured without app/window titles or document content.
3. Close DeskTown before running this isolated demo command:

   ```powershell
   ./launch-demo.ps1 -Scenario workshop-reveal-pending -WindowsExport ./app -DemoSaves ./demo-saves
   ```

   The other scenarios are `workshop-repairing` and `railway-teaser`. Each run
   copies the fixture into a fresh temporary directory and shows an isolated
   demo title. Close the demo normally before running another.
4. For the real focus test, close the demo and launch `./app/DeskTown.exe`.
   Use a separate Windows account or back up any existing DeskTown save before
   destructive recovery checks. Record interaction results in `qa-results.md`
   and follow `WINDOWS_GATE.md`. The headless preflight does not touch the normal
   `user://` save; the real interactive app does.

Ghost is a Town F10 QA preview only and cannot be selected for normal Focus.
The notice is currently enabled by default. Tray can turn it off for Tray-only
completion while keeping `Open DeskTown` available.
