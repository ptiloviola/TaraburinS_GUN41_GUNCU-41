using System;
using UnityEngine;
using UnityEngine.EventSystems;

//Стандартная кнопка в Unity умеет только onClick. 
//Нам нужен крошечный скрипт-помощник, чтобы ловить мышь. 


namespace Gameplay.UI
{
    public class UIHoverListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action OnHoverEnter;
        public event Action OnHoverExit;

        public void OnPointerEnter(PointerEventData eventData) => OnHoverEnter?.Invoke();
        public void OnPointerExit(PointerEventData eventData) => OnHoverExit?.Invoke();
    }
}
