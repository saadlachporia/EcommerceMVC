SELECT ProductId, Name, Description, Price, Stock, ImageData
FROM Products
WHERE ImageData IS NOT NULL;

-- sp_help 'Products';

SELECT ProductId, Name, Description, Price, Stock, ImagePath
FROM Products
WHERE ImagePath IS NOT NULL;

SELECT ProductId, Name, ImagePath FROM Products;
