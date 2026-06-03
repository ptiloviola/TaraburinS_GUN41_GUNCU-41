using UnityEngine;
using Zenject;
using Infrastructure.Signals;
using System;

namespace Gameplay.Economy
{
    // IInitializable и IDisposable нужны, чтобы Zenject сам вызвал Start и OnDestroy для подписок
    public class BankService : MonoBehaviour
    {

    }
}



