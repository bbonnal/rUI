# ADR-0001: AppModel Boundaries

- Status: Accepted
- Date: 2026-02-14

## Context

rUI needs reusable app-state infrastructure (settings + recovery) without coupling UI controls to persistence details.

## Decision

Introduce a dedicated package `rUI.AppModel` that contains only contracts and data envelopes:
- settings contracts,
- recovery contracts,
- serializer/store abstractions.

No concrete persistence implementation is included in this package.

## Consequences

- Positive:
  - clean architecture boundaries,
  - straightforward testability via interfaces,
  - implementation packages can evolve independently.
- Negative:
  - requires additional package(s) before end-to-end runtime behavior is available.
