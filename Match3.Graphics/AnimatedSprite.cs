using Microsoft.Xna.Framework;

namespace Match3.Graphics;

/// <summary>
/// Sprite с frame-by-frame анимацией.
/// Расширяет Sprite, добавляя продвижение кадров по таймеру.
/// </summary>
public class AnimatedSprite : Sprite {
    private int _currentFrame;
    private TimeSpan _elapsed;
    private Animation _animation = null!;

    /// <summary>
    /// Если true — анимация зацикливается. Если false — играет один раз и помечается IsFinished.
    /// </summary>
    public bool Loop { get; set; } = true;

    /// <summary>
    /// True, когда не-зацикленная анимация дошла до последнего кадра.
    /// Полезно для эффектов "проиграл и убрал" (взрыв, вспышка).
    /// </summary>
    public bool IsFinished { get; private set; } = false;

    public Animation Animation {
        get => _animation;
        set {
            _animation = value;
            _currentFrame = 0;
            _elapsed = TimeSpan.Zero;
            IsFinished = false;
            if (_animation.Frames.Count > 0) {
                Region = _animation.Frames[0];
            }
        }
    }

    public AnimatedSprite() { }

    public AnimatedSprite(Animation animation) {
        Animation = animation;
    }

    public void Update(GameTime gameTime) {
        if (IsFinished) return;

        _elapsed += gameTime.ElapsedGameTime;

        if (_elapsed >= _animation.Delay) {
            _elapsed -= _animation.Delay;
            _currentFrame++;

            if (_currentFrame >= _animation.Frames.Count) {
                if (Loop) {
                    _currentFrame = 0;
                } else {
                    _currentFrame = _animation.Frames.Count - 1;
                    IsFinished = true;
                }
            }

            Region = _animation.Frames[_currentFrame];
        }
    }
}
