# Key Layout

`rUI.AppModel.Json` maps logical keys to file paths under `RootDirectory`.

## Settings

Logical key:
- `settings/{contract}`

Physical path:
- `<root>/settings/<normalized-contract>.json`

## Recovery

Logical key:
- `recovery/{contract}/{scope}`

Physical path:
- `<root>/recovery/<normalized-contract>/<normalized-scope>.json`

## Normalization Rules

- path separators in key become directory separators,
- unsafe characters are replaced with `_`,
- empty normalized segment becomes `default`.

This avoids traversal issues and keeps paths portable.
