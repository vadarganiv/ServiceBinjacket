#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

# Load env vars from project .env
if [ -f "$PROJECT_DIR/.env" ]; then
    set -a
    # shellcheck source=/dev/null
    source "$PROJECT_DIR/.env"
    set +a
fi

BACKUP_DIR="${BACKUP_DIR:-/backups}"
CONTAINER="servis-binjaket-postgres"

mkdir -p "$BACKUP_DIR"

ts=$(date +%Y%m%d-%H%M%S)
OUTFILE="$BACKUP_DIR/db-$ts.sql.gz"

echo "Backing up database '$POSTGRES_DB' → $OUTFILE"
docker exec "$CONTAINER" pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB" | gzip > "$OUTFILE"
echo "Done. Size: $(du -sh "$OUTFILE" | cut -f1)"

# Remove backups older than 30 days
find "$BACKUP_DIR" -name 'db-*.sql.gz' -mtime +30 -delete
echo "Old backups (>30 days) cleaned up."
