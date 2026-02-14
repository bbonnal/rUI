# Selected Decisions

Decision set selected on 2026-02-14:

1. Schema version format
- Selected: `int`.

2. Partition model
- Selected: single-profile first.

3. Recovery startup policy
- Selected: prompt-based restore (policy to be enforced in orchestration layer).

4. First provider implementation
- Selected: JSON + file store first.

5. Corruption handling
- Selected: quarantine bad payload and continue with defaults.

These defaults are now implemented in `rUI.AppModel.Json` contracts/runtime behavior.
