﻿namespace Client.UseCases.eShop.TransactionInput
{
    public class DeleteProductTransactionInput : IInput
    {

        public int NumTotalItems { get; set; }
        public string CatalogUrl { get; set; }

    }
}
