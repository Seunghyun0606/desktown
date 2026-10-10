import os
from pathlib import Path
import sys
import tempfile
import unittest
from unittest.mock import patch

from export_windows import prepare_output, require_artifacts, run_checked


class ExportValidationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.directory = Path(self.temp.name)

    def invoke(self, code, timeout=10):
        return run_checked([sys.executable, "-c", code], self.directory, os.environ.copy(), self.directory / "run.log", timeout)

    def test_zero_exit_with_godot_error_is_failure(self):
        with self.assertRaisesRegex(RuntimeError, "despite exit 0"):
            self.invoke("print('\\x1b[31mERROR: publish failed\\x1b[0m')")

    def test_nonzero_exit_is_failure(self):
        with self.assertRaisesRegex(RuntimeError, "exited with 7"):
            self.invoke("raise SystemExit(7)")

    def test_timeout_is_failure_even_with_partial_artifact(self):
        with self.assertRaisesRegex(RuntimeError, "Timed out"):
            self.invoke("from pathlib import Path; import time; Path('DeskTown.exe').write_bytes(b'partial'); time.sleep(20)", timeout=1)
        self.assertTrue((self.directory / "DeskTown.exe").exists())

    def test_executable_alone_is_not_complete_export(self):
        (self.directory / "DeskTown.exe").write_bytes(b'partial')
        with self.assertRaisesRegex(RuntimeError, "DeskTown.dll"):
            require_artifacts(self.directory)

    def test_output_outside_build_is_rejected_without_modification(self):
        (self.directory / "preserve.txt").write_text("keep", encoding="utf-8")
        with self.assertRaises(ValueError):
            prepare_output(self.directory)
        self.assertEqual((self.directory / "preserve.txt").read_text(encoding="utf-8"), "keep")

    def test_successful_command_preserves_log(self):
        self.assertIn("Export complete", self.invoke("print('Export complete')"))

    def test_existing_export_is_preserved(self):
        output = self.directory / "build/app"
        output.mkdir(parents=True)
        artifact = output / "DeskTown.exe"
        artifact.write_bytes(b"existing")
        with patch("export_windows.ROOT", self.directory):
            with self.assertRaisesRegex(ValueError, "must be empty"):
                prepare_output(output)
        self.assertEqual(artifact.read_bytes(), b"existing")


if __name__ == "__main__":
    unittest.main()
