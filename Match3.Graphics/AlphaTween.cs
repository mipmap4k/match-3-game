using Microsoft.Xna.Framework;

namespace Match3.Graphics;

/// <summary>
/// Анимация плавного изменения числа (для прозрачности или scale)
/// от Start до End за указанное время.
/// </summary>
public class AlphaTween {
    public float Start { get; set; }
    public float End { get; set; }
    public float Duration { get; set; }
    public float Elapsed { get; private set; }

    public bool IsFinished => Elapsed >= Duration;

    /// <summary>
    /// Текущее интерполированное значение (Lerp от Start до End).
    /// </summary>
    public float Current {
        get {
            if (Duration <= 0f) return End;
            float t = MathHelper.Clamp(Elapsed / Duration, 0f, 1f);
            return MathHelper.Lerp(Start, End, t);
        }
    }

    public AlphaTween(float start, float end, float durationSeconds) {
        Start = start;
        End = end;
        Duration = durationSeconds;
        Elapsed = 0f;
    }

    public void Update(GameTime gameTime) {
        if (IsFinished) return;
        Elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}
