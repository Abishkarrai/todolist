## Summary
- Change type: Feature / Fix / Refactor / Security / Infrastructure
- Business context:
- Related ticket: 

## Work Item / Traceability
<!-- Mandatory for audit -->
- Jira / Azure DevOps ID: 
- Link: 

## Security Checklist
- [ ] No hardcoded secrets (validated by Gitleaks)
- [ ] Uses Azure Key Vault / Managed Identity
- [ ] Input validation enforced on all endpoints
- [ ] AuthN/AuthZ verified
- [ ] No PII logged
- [ ] Environment variables documented
- [ ] Feature flags used (if applicable)
- [ ] Tenant isolation maintained

## Architecture Impact
- Affected controllers/services:
- API breaking changes: Yes / No
- Database migration: Yes / No

## Testing Evidence
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Security scans passed (SAST, SCA, secrets)
- [ ] Manual testing completed
- [ ] All CI gates passed
- Pipeline Run URL:

## Artifact & Supply Chain
- [ ] SBOM generated
- [ ] Artifact signed
- [ ] Dependencies reviewed (no critical vulnerabilities)

## Observability
- [ ] Logging added/updated
- [ ] Metrics added (if applicable)
- [ ] Alerts configured (if required)

## Rollback Plan
- Method: Revert PR / Redeploy previous artifact / Disable feature flag
- Data rollback required: Yes / No
- Estimated time: <5 min / <15 min

## Soak Period
- Stage soak: 24-48h
- Prod canary (Ring 1) soak: 12-24h
- Exit criteria: Error rate < 0.1%, P95 latency stable

## Observability
- [ ] Structured logging added (tenant-aware)
- [ ] Metrics updated in App Insights
- [ ] Alerts configured
