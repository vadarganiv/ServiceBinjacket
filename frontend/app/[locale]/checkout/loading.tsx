export default function Loading() {
  return (
    <div className="container mx-auto px-4 py-8">
      <div className="h-8 w-48 bg-muted rounded-lg animate-pulse mb-8" />
      <div className="max-w-4xl mx-auto grid grid-cols-1 lg:grid-cols-[1fr_360px] gap-8">
        <div className="space-y-6">
          {[1, 2, 3].map(i => (
            <div key={i} className="space-y-3">
              <div className="h-5 w-40 bg-muted rounded animate-pulse" />
              <div className="h-10 bg-muted rounded-xl animate-pulse" />
              <div className="h-10 bg-muted rounded-xl animate-pulse" />
            </div>
          ))}
        </div>
        <div className="h-64 bg-muted rounded-2xl animate-pulse" />
      </div>
    </div>
  );
}
