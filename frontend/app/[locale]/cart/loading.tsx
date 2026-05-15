export default function Loading() {
  return (
    <div className="container mx-auto px-4 py-8 max-w-2xl">
      <div className="h-8 w-32 bg-muted rounded-lg animate-pulse mb-8" />
      <div className="space-y-4">
        {[1, 2, 3].map(i => (
          <div key={i} className="flex gap-4 py-4 border-b border-border">
            <div className="w-20 h-20 bg-muted rounded-xl animate-pulse flex-shrink-0" />
            <div className="flex-1 space-y-2">
              <div className="h-4 bg-muted rounded animate-pulse w-3/4" />
              <div className="h-4 bg-muted rounded animate-pulse w-1/4" />
              <div className="h-8 bg-muted rounded-lg animate-pulse w-24" />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
