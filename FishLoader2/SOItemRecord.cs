using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishLoader2
{
    class SOItemRecord
    {
        private int _fbId;
        private int _productId;
        private int _soId;
        private int _typeId;
        private int _statusId;
        private int _uomId;
        private int _soLineItem;
        private string _description;
        private string _productMum;
        private string _customerPartNum;
        private int _taxableFlag;
        private int _taxId;
        private double _qtyToFulfill;
        private double _qtyFulfilled;
        private double _qtyPicked;
        private double _unitPrice;
        private double _totalPrice;
        private DateTime _dateLastFulfillment;
        private DateTime _dateScheduledFulfillment;
        private string _revLevel;
        private int _exchangeSOLineItem;
        private double _adjustAmount;
        private double _adjustPercentage;
        private int _qbClassId;
        private string _note;
        private double _totalCost;
        private int _showItemFlag;
        private int _itemAdjustId;
        private double _mcTotalPrice;
        private double _markupCost;

        public int FBId
        {
            get { return _fbId; }
            set { _fbId = value; }
        }


        public int ProductId
        {
            get { return _productId; }
            set { _productId = value; }
        }


        public int SOId
        {
            get { return _soId; }
            set { _soId = value; }
        }

        public int TypeId
        {
            get { return _typeId; }
            set { _typeId = value; }
        }

        public int StatusId
        {
            get { return _statusId; }
            set { _statusId = value; }
        }


        public int UOMId
        {
            get { return _uomId; }
            set { _uomId = value; }
        }

        public int SOLineItem
        {
            get { return _soLineItem; }
            set { _soLineItem = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string ProductNum
        {
            get { return _productMum; }
            set { _productMum = value; }
        }


        public string CustomerPartNum
        {
            get { return _customerPartNum; }
            set { _customerPartNum = value; }
        }


        public int TaxableFlag
        {
            get { return _taxableFlag; }
            set { _taxableFlag = value; }
        }


        public int TaxId
        {
            get { return _taxId; }
            set { _taxId = value; }
        }


        public double QtyToFulfill
        {
            get { return _qtyToFulfill; }
            set { _qtyToFulfill = value; }
        }

        public double QtyFulfilled
        {
            get { return _qtyFulfilled; }
            set { _qtyFulfilled = value; }
        }

        public double QtyPicked
        {
            get { return _qtyPicked; }
            set { _qtyPicked = value; }
        }


        public double UnitPrice
        {
            get { return _unitPrice; }
            set { _unitPrice = value; }
        }


        public double TotalPrice
        {
            get { return _totalPrice; }
            set { _totalPrice = value; }
        }


        public DateTime DateLastFulfillment
        {
            get { return _dateLastFulfillment; }
            set { _dateLastFulfillment = value; }
        }


        public DateTime DateScheduledFulfillment
        {
            get { return _dateScheduledFulfillment; }
            set { _dateScheduledFulfillment = value; }
        }


        public string RevLevel
        {
            get { return _revLevel; }
            set { _revLevel = value; }
        }


        public int ExchangeSOLineItem
        {
            get { return _exchangeSOLineItem; }
            set { _exchangeSOLineItem = value; }
        }


        public double AdjustAmount
        {
            get { return _adjustAmount; }
            set { _adjustAmount = value; }
        }


        public double AdjustPercentage
        {
            get { return _adjustPercentage; }
            set { _adjustPercentage = value; }
        }


        public int QBClassId
        {
            get { return _qbClassId; }
            set { _qbClassId = value; }
        }

        public string Note
        {
            get { return _note; }
            set { _note = value; }
        }

        public double TotalCost
        {
            get { return _totalCost; }
            set { _totalCost = value; }
        }


        public int ShowItemFlag
        {
            get { return _showItemFlag; }
            set { _showItemFlag = value; }
        }


        public int ItemAdjustId
        {
            get { return _itemAdjustId; }
            set { _itemAdjustId = value; }
        }

        public double McTotalPrice
        {
            get { return _mcTotalPrice; }
            set { _mcTotalPrice = value; }
        }



        public double MarkupCost
        {
            get { return _markupCost; }
            set { _markupCost = value; }
        }


    }
}

