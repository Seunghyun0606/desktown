import copy
import json
import unittest

from validate_catalog import CATALOG, manifest_records, validate


class CatalogContractTests(unittest.TestCase):
    def setUp(self):
        self.manifest = manifest_records()
        self.catalog = json.loads(CATALOG.read_text(encoding="utf-8"))

    def test_catalog_matches_all_manifest_ids(self):
        self.assertEqual(46, len(self.manifest))
        self.assertEqual([], validate(self.catalog, self.manifest))

    def test_dimension_drift_and_duplicate_ids_fail(self):
        changed = copy.deepcopy(self.catalog)
        changed["assets"][0]["size"] = [1, 1]
        changed["assets"][1]["id"] = changed["assets"][0]["id"]
        errors = validate(changed, self.manifest)
        self.assertTrue(any("size differs" in error for error in errors))
        self.assertTrue(any("unique manifest IDs" in error for error in errors))

    def test_unassigned_catalog_cannot_claim_production_ready(self):
        self.assertEqual(46, len(validate(self.catalog, self.manifest, True)))


if __name__ == "__main__":
    unittest.main()
