# Documents, signatures, and evidence

[Home](../README.md) · [Security](../security/README.md) · [Integrations](../integrations/README.md)

> **Current implementation (audited 2026-10-12):** Metadata only (`matter_documents`, with a caller-supplied `StorageKey` and review status). No binary storage, scanning, versions, holds, signatures, or downloads. See G32–G33 in the [gap map](../delivery/implementation-gap-map.md).

Documents have immutable versions, classification, matter/firm ownership, uploader, source, scan status, access grants, retention, hold state, and private object-version references. Store originals separately from previews, OCR output, redacted copies, and productions. A file name is untrusted metadata, not a storage path.

Upload flow: authorize request → allocate opaque scoped key → upload under size/type constraints → quarantine → validate content and scan → mark available → generate safe previews/OCR → permit audience publication. Failed/unknown scan status remains unavailable. Handle password-protected files, decompression bombs, macros, malformed PDFs, duplicate uploads, and scanning outages explicitly.

Download/preview/export reauthorize before issuing short-lived private access. Expiration limits exposure; it does not provide immediate revocation of already issued URLs. For content requiring immediate revocation, proxy access or apply an equivalent enforced mechanism. Avoid confidential information in object keys and URLs.

## Evidence preservation
Record original checksum, source, collection date, collector, custody events, access, transformation history, and exact production version. Preserve metadata relevant to evidentiary integrity; a hash alone does not prove admissibility or completeness. Legal holds override ordinary deletion and lifecycle expiration. Bates labeling, redaction, and production manifests refer to copies while preserving originals.

## Signatures
Use a reviewed provider contract and permitted embedded flow. Track envelope version, required signers, identity/authentication evidence, sent/completed/declined/expired states, final signed document, and completion certificate. A request acceptance response is not completed signature evidence. Reject stale or duplicate callbacks and preserve the signed version.

Acceptance: wrong-firm file requests fail; malicious files cannot be downloaded while quarantined; legal hold blocks deletion; changed document versions require new approval/signature when relevant; previews and OCR never bypass source-document access rules.
