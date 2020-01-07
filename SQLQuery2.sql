--SELECT Id, PurchaseDate, DecomissionDate, 
--                                       Make, Model
--                                       FROM Computer WHERE DecomissionDate IS 
--                                       NULL;

--SELECT c.Id as ComputerId, PurchaseDate, DecomissionDate, Make, Model, e.Id as EmployeeId
--                                           FROM Computer c 
--                                           LEFT JOIN Employee e ON e.ComputerId = c.Id
--                                           WHERE DecomissionDate IS NOT NULL OR e.Id IS NOT NULL

--SELECT Id, PurchaseDate, DecomissionDate, Make, Model
--                                        FROM Computer

INSERT INTO Computer (Make, Model, PurchaseDate, DecomissionDate)
OUTPUT INSERTED.id
VALUES (@Make, @Model, @PurchaseDate, @DecomissionDate);