export default function ServiceCardSkeleton() {
  return (
    <div className="flex flex-col bg-background border border-border rounded-xl overflow-hidden animate-pulse">
      <div className="h-32 bg-muted" />
      <div className="flex flex-col p-4 gap-2">
        <div className="h-3 w-20 bg-muted rounded" />
        <div className="h-4 w-full bg-muted rounded" />
        <div className="h-3 w-4/5 bg-muted rounded" />
        <div className="h-3 w-3/5 bg-muted rounded" />
      </div>
    </div>
  );
}
