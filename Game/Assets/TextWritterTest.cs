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
        private float _currentTime;
        private int _characterIndex = 0;

        private TextRenderer _textRenderer;

        public override void OnStart()
        {
            _textRenderer = GetComponent<TextRenderer>();
            _textRenderer.Font = Assets.Get<FontAsset>("Fonts/windows-bold[1].ttf");
            _textRenderer.FontSize = 30;

            for (int i = 0; i < Text.Length; i++)
            {
                if (Text[i] == '\n')
                {
                    _textRenderer.Text.Append('\n');
                }
                else
                    _textRenderer.Text.Append('\0');
            }
        }

        public override void OnUpdate()
        {
            if((_currentTime -= Time.DeltaTime) <= 0)
            {
                _currentTime = DelayToWrite;
                if (_textRenderer.Text.Length != _characterIndex && Text.Length > _characterIndex)
                {
                    _textRenderer.Text[_characterIndex] = Text[_characterIndex];
                    _characterIndex++;
                }

            }
        }
    }
}
