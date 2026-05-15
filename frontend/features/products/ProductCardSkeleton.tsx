export default function ProductCardSkeleton() {
  return (
    <div className="flex flex-col bg-background border border-border rounded-xl overflow-hidden animate-pulse">
      <div className="aspect-square bg-muted" />
      <div className="p-4 flex flex-col gap-3">
        <div className="h-4 bg-muted rounded w-3/4" />
        <div className="h-3 bg-muted rounded w-full" />
        <div className="h-3 bg-muted rounded w-2/3" />
        <div className="mt-2 flex items-center justify-between">
          <div className="h-5 bg-muted rounded w-1/3" />
          <div className="h-5 bg-muted rounded-full w-1/4" />
        </div>
      </div>
    </div>
  );
}
