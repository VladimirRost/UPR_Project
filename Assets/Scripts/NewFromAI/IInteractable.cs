using UnityEngine;

// Интерфейс — это "контракт".
// Любой объект, который можно использовать,
// должен иметь метод Interact().

public interface IInteractable
{
    void Interact(); // Вызов по нажатию кнопки
    void OnFocus();
    void OnLoseFocus();
}