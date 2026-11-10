CREATE TABLE ProductsByPopularity (
    select ProductId, Name, Price, StockQuantity, Rating, InquiryFrequency
    from Products
    where sold > InquiryFrequency
)

