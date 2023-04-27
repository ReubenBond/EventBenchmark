﻿using System;
namespace Common.Entity
{
    /**
     * Seller information is assembled based on two sources:
     * (i) Olist dev public API: https://dev.olist.com/docs/retrieving-seller-informations
     * (ii) Olist public data set: https://www.kaggle.com/datasets/olistbr/brazilian-ecommerce?select=olist_sellers_dataset.csv
     * The additional attributes added as part of this benchmark are:
     * street, complement, order_count
     */
    public class Seller
    {

        public long id { get; set; }

        public string name { get; set; }

        public string company_name { get; set; }

        public string email { get; set; }

        public string phone { get; set; }

        public string mobile_phone { get; set; }

        public string cpf { get; set; }

        public string cnpj { get; set; }

        public string address { get; set; }

        public string complement { get; set; }

        public string city { get; set; }

        public string state { get; set; }

        public string zip_code_prefix { get; set; }

        // statistics. these vakues can be deduced (by aggregate operation) from order items and packages, respectively. not sure we should keep them
        public int order_count { get; set; }
        // public int delivery_count { get; set; }

    }
}

