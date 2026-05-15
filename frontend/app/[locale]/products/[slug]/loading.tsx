export default function ProductDetailLoading() {
  return (
    <div className="container mx-auto px-4 py-8 animate-pulse">
      <div className="h-5 w-44 bg-muted rounded mb-6" />
      <div className="grid grid-cols-1 md:grid-cols-2 gap-8 lg:gap-12">
        <div className="aspect-square bg-muted rounded-2xl" />
        <div className="flex flex-col gap-5">
          <div className="flex gap-2 mb-1">
            <div className="h-6 w-16 bg-muted rounded-full" />
            <div className="h-6 w-20 bg-muted rounded-full" />
          </div>
          <div className="h-8 bg-muted rounded w-3/4" />
          <div className="h-10 bg-muted rounded w-1/3" />
          <div className="space-y-2">
            <div className="h-4 bg-muted rounded w-full" />
            <div className="h-4 bg-muted rounded w-4/5" />
          </div>
          <div className="h-12 bg-muted rounded-xl w-full mt-4" />
        </div>
      </div>
    </div>
  );
}
