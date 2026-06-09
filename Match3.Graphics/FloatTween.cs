using Microsoft.Xna.Framework;

namespace Match3.Graphics;

/// <summary>
/// Анимация плавного изменения float-значения с опциональной задержкой старта.
/// Удобно для альфа-канала, scale, повёрнутости.
/// </summary>
public class FloatTween {
    public float Start { get; set; }
    public float End { get; set; }
    public float Duration { get; set; }
    public float StartDelay { get; set; }
    public float Elapsed { get; private set; }

    public bool IsFinished => Elapsed >= Duration + StartDelay;

    /// <summary>
    /// Текущее значение. Возвращает Start пока StartDelay не истёк, потом Lerp(Start, End, t).
    /// </summary>
    public float Current {
        get {
            if (Elapsed < StartDelay) return Start;
            if (Duration <= 0f) return End;
            float t = MathHelper.Clamp((Elapsed - StartDelay) / Duration, 0f, 1f);
            return MathHelper.Lerp(Start, End, t);
        }
    }

    public FloatTween(float start, float end, float durationSeconds, float startDelaySeconds = 0f) {
        Start = start;
        End = end;
        Duration = durationSeconds;
        StartDelay = startDelaySeconds;
        Elapsed = 0f;
    }

    public void Update(GameTime gameTime) {
        if (IsFinished) return;
        Elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}
