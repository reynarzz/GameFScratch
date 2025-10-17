using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class TextWritterTest : ScriptBehavior
    {
        public string Text { get; set; }
        public float DelayToWrite { get; set; } = 0.1f;
        public float TextSize { get; set; } = 30;
        public Color Color { get; set; } = Color.White;
        private float _currentTime;
        private int _characterIndex = 0;

        private TextRenderer _textRenderer;

        public override void OnStart()
        {
            var actor = new Actor<TextRenderer>();
            _textRenderer = actor.GetComponent<TextRenderer>();
            _textRenderer.Font = Assets.Get<FontAsset>("Fonts/windows-bold[1].ttf");
            _textRenderer.Color = Color;
            _textRenderer.FontSize = TextSize;
        }

        public override void OnUpdate()
        {
            if((_currentTime -= Time.DeltaTime) <= 0)
            {
                _currentTime = DelayToWrite;
                if (string.IsNullOrEmpty(_textRenderer.Text) || _textRenderer.Text.Length != Text.Length && Text.Length > _characterIndex)
                {
                    _textRenderer.Text += Text[_characterIndex];
                    _characterIndex++;
                }
            }
        }
    }
}
