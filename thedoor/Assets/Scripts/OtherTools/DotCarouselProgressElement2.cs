using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheDoor.Main {
    /// <summary>
    ///     Element of the <see cref="DotCarouselProgressView" />.
    /// </summary>
    public class DotCarouselProgressElement2 : MonoBehaviour {
        [SerializeField] private Image _image;
        [SerializeField] Sprite ActiveSprite;
        [SerializeField] Sprite InActiveSprite;
        [SerializeField] private Button _button;

        /// <summary>
        ///     Active or not.
        /// </summary>
        public bool IsActive { get; private set; }

        public Button Button => _button;

        /// <summary>
        ///     Set whether or not it is active.
        /// </summary>
        /// <param name="isActive"></param>
        public void SetActive(bool isActive) {
            _image.sprite = isActive ? ActiveSprite : InActiveSprite;
            IsActive = isActive;
        }
    }
}
