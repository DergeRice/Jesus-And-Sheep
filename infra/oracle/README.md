# EC2 → OCI Always Free migration

Prepared infrastructure, not a completed deployment. The Unity repository does not include the API server, database schema, credentials or a data export. Do not switch DNS or remove EC2 before those have been migrated and verified.

## Cost and prerequisites

This module provisions one ARM A1 VM (1 OCPU, 6 GB RAM, 50 GB boot volume), a VCN, subnet, routing and firewall. It deliberately has no paid fallback shape, NAT gateway or load balancer. Use your tenancy's **home region**, an Always Free eligible Ubuntu 22.04 ARM64 image, and check existing account-wide allocation before applying. Current Oracle documentation lists 1,500 OCPU-hours and 9,000 GB-hours monthly for A1 and 200 GB combined boot/block storage. Capacity may be unavailable and idle free VMs may be reclaimed; keep off-host database backups. This configuration alone cannot guarantee the total bill for an existing account.

Source checked 2026-09-16: https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier_topic-Always_Free_Resources.htm

## Provision

1. Configure OCI API-key credentials locally in `~/.oci/config`; never commit private keys or put them in Unity Resources.
2. Copy `terraform.tfvars.example` to `terraform.tfvars` and replace all values. The selected image must be ARM64, not an EC2/x86 image.
3. Run `terraform init`, `terraform fmt -check`, `terraform validate`, and `terraform plan -out=migration.tfplan`. Review account quotas and the planned resources.
4. Run `terraform apply migration.tfplan`. SSH as `ubuntu` to `terraform output -raw public_ip`. Wait for `sudo cloud-init status --wait` and inspect `/var/log/cloud-init-output.log`.
5. `/healthz` initially reports **host-ready; backend-not-deployed**. All other routes return 503 until the actual application is installed.

## Migrate the actual application

- Obtain the EC2 backend repository, runtime/version, environment-variable names, database engine/schema/export, backup/restore procedure, and monthly leaderboard reset job.
- Build the application for ARM64 or use a multi-architecture container. Bind the application only to `127.0.0.1:3000`; do not publish database ports. Restore data and mount durable database volumes. Provision secrets on the host, outside Terraform state and the Unity project.
- Verify backend startup and database health locally. Copy `Caddyfile.example` to `/etc/caddy/Caddyfile`, use a staging subdomain pointing to this VM, and run `sudo caddy validate --config /etc/caddy/Caddyfile` then `sudo systemctl reload caddy`. Caddy obtains HTTPS certificates after DNS resolves to the VM.
- Preserve the existing `/api2` prefix. Do not strip it unless the backend router is confirmed to require that change.
- Verify the contracts below against staging. Use disposable test users for mutations. Confirm authentication/ownership and duplicate-request behavior in the original backend; the client currently does not send authentication headers.
- Back up the existing DB, pause writes for the final export/restore, test the staged application, then move the existing API hostname to OCI. Retaining the hostname allows existing installed clients to migrate without an app update. Check for other services on that hostname before changing DNS.
- If changing hostname instead, set `Assets/Resources/Network/server.json` → `baseUrl` to the new HTTPS URL including `/api2` and build an updated client. Empty `baseUrl` intentionally keeps the existing scene URL until migration is ready; it does not move traffic by itself.
- Keep EC2/data available for rollback until new and old clients, TLS, ranking, mail, gifts and scheduled jobs are verified. Restore the old DNS and reconcile writes if rolling back. The VM has Terraform `prevent_destroy` and preserves its boot disk; deliberate removal needs a separate reviewed change.

## Existing client contract

| Method | Path (under `/api2`) | Request | Expected response |
|---|---|---|---|
| POST | `/versionCheck` | form `version` | JSON boolean `isCompatible` |
| GET | `/check` | query `nickname` | exact `Nickname is available.` (verified on the existing host), `available`, or JSON boolean `available` / exact status or message `available` |
| POST | `/enroll/` | JSON User | 2xx only on successful registration |
| GET | `/getRanking` | none | array of User objects |
| POST | `/update` | form `nickname`, `highscore` | 2xx on success |
| POST | `/sendHeart` | JSON `senderNickName`, `targetNickName` | 2xx on success |
| GET | `/checkMyHeartList` | query `nickname` | array of HeartData objects |
| POST | `/receiveHeart` | JSON `heartHash` | 2xx only on the first successful claim |

Nickname availability and version compatibility (`2.74` → `{"isCompatible":true}`) were verified with read-only checks against the existing host on 2026-09-16. Other contracts are inferred from the client, not verified against the missing server. Confirm all responses before cutover. Preserve IDs, nicknames, high scores, profile indexes, pending gift hashes and timestamps when migrating.
