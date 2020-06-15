using System;
using Services;
using UnityEngine;

public class ServiceLocatorInitializer : MonoBehaviour
{
    private void Awake()
    {
        ServiceLocator.Register<IRoomSyncService, RoomSyncService>();
    }
}
