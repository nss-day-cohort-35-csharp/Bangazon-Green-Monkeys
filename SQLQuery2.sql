<<<<<<< HEAD
﻿SELECT Id FROM [Order]
WHERE CustomerId =1 AND UserPaymentTypeId IS NULL;
=======
﻿--SELECT Id, PurchaseDate, DecomissionDate, 
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
>>>>>>> 15fa81b5e9223e5a3b3269f39be34223ea73c627
