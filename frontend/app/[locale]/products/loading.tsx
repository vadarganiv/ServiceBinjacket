import ProductCardSkeleton from '@/features/products/ProductCardSkeleton';

export default function ProductsLoading() {
  return (
    <div className="container mx-auto px-4 py-8 animate-pulse">
      <div className="h-9 w-36 bg-muted rounded-lg mb-8" />
      <div className="h-10 w-full max-w-sm bg-muted rounded-xl mb-5" />
      <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-3 md:gap-4">
        {Array.from({ length: 8 }).map((_, i) => (
          <ProductCardSkeleton key={i} />
        ))}
      </div>
    </div>
  );
}
