using System;

// delegate TResult UnlessBody(bool expression);

public class Util {

    // public Unless unless = delegate (bool expresion) {

    // }
    // public bool unless(bool expression, Func<T> func) {
    //     if (!expression)
    //         return true;
    //     else {
    //         return false;
    //     }
    // }

    public void ShowMenu(CanvasGroup group) {
        group.alpha = 1.0f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    public void HideMenu(CanvasGroup group) {
        group.alpha = 0.0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }
}