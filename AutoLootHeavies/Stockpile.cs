﻿using UnityEngine;

namespace AutoLootHeavies;

public class Stockpile
{
    private readonly Vector3 _location;
    private readonly StockpileType _type;
    private float _distanceFromPlayer;
    private readonly WorldGameObject _wgo;

    internal Stockpile(Vector3 location, StockpileType type, float distanceFromPlayer, WorldGameObject wgo)
    {
        _location = location;
        _type = type;
        _distanceFromPlayer = distanceFromPlayer;
        _wgo = wgo;
    }

    internal enum StockpileType
    {
        Timber,
        Ore,
        Stone,
        Unknown
    }

    public WorldGameObject GetStockpileObject()
    {
        return _wgo;
    }

    public Vector3 GetLocation()
    {
        return _location;
    }

    internal StockpileType GetStockpileType()
    {
        return _type;
    }

    public float GetDistanceFromPlayer()
    {
        return _distanceFromPlayer;
    }

    public void SetDistanceFromPlayer(float distance)
    {
        _distanceFromPlayer = distance;
    }
}