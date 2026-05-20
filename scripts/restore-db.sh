#!/usr/bin/env bash
set -euo pipefail

if [ -z "${1:-}" ]; then
    echo "Usage: $0 /path/to/backup.sql.gz"
    echo ""
    echo "Example: $0 /backups/db-20260515-030000.sql.gz"
    exit 1
fi

BACKUP_FILE="$1"

if [ ! -f "$BACKUP_FILE" ]; then
    echo "Error: file not found: $BACKUP_FILE"
    exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

# Load env vars from project .env
if [ -f "$PROJECT_DIR/.env" ]; then
    set -a
    # shellcheck source=/dev/null
    source "$PROJECT_DIR/.env"
    set +a
fi

CONTAINER="servis-binjaket-postgres"

echo "Restoring '$BACKUP_FILE' → database '$POSTGRES_DB' in container '$CONTAINER'"
echo "WARNING: This will overwrite existing data. Press Ctrl+C within 5 seconds to cancel."
sleep 5

gunzip -c "$BACKUP_FILE" | docker exec -i "$CONTAINER" psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"
echo "Restore complete."
