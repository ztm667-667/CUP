using HearthCup.Application;
using HearthCup.Domain;
var repo=new FakeRepository();var shop=new ShopService(repo);var passed=0;
async Task Check(string name,Func<Task> run){await run();Console.WriteLine($"PASS {name}");passed++;}
void Assert(bool condition){if(!condition)throw new Exception("Assertion failed");}
async Task Reject(Func<Task> run){try{await run();}catch(ShopRuleException){return;}throw new Exception("Expected business rejection");}
ProductInput Input()=>new(){Name="Test family cup",Description="A comfortable ceramic cup for the family table.",CapacityMl=350,Price=14,Stock=4};
await Check("Add accepts available quantity",async()=>Assert((await shop.AddAsync([],1,2)).Single().Quantity==2));
await Check("Add merges duplicate lines",async()=>Assert((await shop.AddAsync([new(1,2)],1,1)).Single().Quantity==3));
await Check("Reject cumulative quantity above stock",()=>Reject(async()=>await shop.AddAsync([new(1,3)],1,2)));
await Check("Reject zero quantity",()=>Reject(async()=>await shop.AddAsync([],1,0)));
await Check("Reject negative quantity",()=>Reject(async()=>await shop.AddAsync([],1,-1)));
await Check("Reject excessive quantity",()=>Reject(async()=>await shop.AddAsync([],1,100)));
await Check("Reject nonexistent product",()=>Reject(async()=>await shop.AddAsync([],999,1)));
await Check("Reject sold-out product",()=>Reject(async()=>await shop.AddAsync([],2,1)));
await Check("Basket uses current server price",async()=>{var c=await shop.ReadCartAsync([new(1,2)]);Assert(c.Total==28&&c.Count==2);repo.Products[0].Price=15;c=await shop.ReadCartAsync([new(1,2)]);Assert(c.Total==30);});
await Check("Basket clamps stock and removes deleted products",async()=>{var c=await shop.ReadCartAsync([new(1,9),new(999,1),new(2,1)]);Assert(c.Changed&&c.Lines.Count==1&&c.Count==4);});
await Check("Reject negative price",()=>{var i=Input();i.Price=-1;return Reject(async()=>await shop.SaveAsync(null,i));});
await Check("Reject fractional penny",()=>{var i=Input();i.Price=1.001m;return Reject(async()=>await shop.SaveAsync(null,i));});
await Check("Reject negative stock",()=>{var i=Input();i.Stock=-1;return Reject(async()=>await shop.SaveAsync(null,i));});
await Check("Reject unapproved image path",()=>{var i=Input();i.Design="../../private";return Reject(async()=>await shop.SaveAsync(null,i));});
await Check("Create normalizes name and saves valid product",async()=>{var i=Input();i.Name="  Family cup  ";await shop.SaveAsync(null,i);Assert(repo.Saved?.Name=="Family cup");});
await Check("Edit requires a valid concurrency token",()=>Reject(async()=>await shop.SaveAsync(1,Input())));
await Check("Delete rejects malformed version",()=>Reject(async()=>await shop.DeleteAsync(1,"not-base64")));
Console.WriteLine($"{passed} business checks passed.");
sealed class FakeRepository:IProductRepository
{
 public List<Product> Products {get;}=[new(){Id=1,Name="Sunday Mug",Price=14,Stock=4},new(){Id=2,Name="Sold-out Mug",Price=20,Stock=0}];
 public ProductInput? Saved {get;private set;}
 public Task<Product?> FindAsync(int id,CancellationToken ct=default)=>Task.FromResult(Products.FirstOrDefault(p=>p.Id==id));
 public Task<IReadOnlyList<Product>> FindManyAsync(IEnumerable<int> ids,CancellationToken ct=default)=>Task.FromResult<IReadOnlyList<Product>>(Products.Where(p=>ids.Contains(p.Id)).ToArray());
 public Task<CataloguePage> SearchAsync(CatalogueQuery q,CancellationToken ct=default)=>Task.FromResult(new CataloguePage([],0,1,8));
 public Task<int> SaveAsync(int? id,ProductInput input,byte[]? version,CancellationToken ct=default){Saved=input;return Task.FromResult(id??3);}
 public Task DeleteAsync(int id,byte[] version,CancellationToken ct=default)=>Task.CompletedTask;
}
