﻿using System.Collections.Generic;

namespace GerrysJunkTrunk;

public partial class Plugin
{
    private const float FullPriceModifier = 0.90f;
    private const float PityPrice = 0.10f;
    private const int LargeInvSize = 20;
    private const int LargeMaxItemCount = 100;
    private const float PriceModifier = 0.60f;
    private const string ShippingBoxTag = "shipping_box";
    private const string ShippingItem = "shipping";
    private const int SmallInvSize = 10;
    private const int SmallMaxItemCount = 50;
    private static readonly List<WorldGameObject> KnownVendors = new();
    private static int _techCount;
    private static int _oldTechCount;
    private static readonly Dictionary<string, int> StackSizeBackups = new();

    //private static readonly List<WorldGameObject> VendorWgos = new();
    private static WorldGameObject _myVendor;
    private static WorldGameObject _shippingBox;
    private static WorldGameObject _interactedObject;
    private static bool _shippingBuild;
    private static bool _usingShippingBox;
    private static List<VendorSale> _vendorSales = new();
    private static readonly List<ItemPrice> PriceCache = new();
    private static readonly List<BaseItemCellGUI> AlreadyDone = new();
    private static ObjectCraftDefinition _newItem;
    private const string ShippingBoxId = "mf_wood_builddesk:p:mf_shipping_box_place";  
}