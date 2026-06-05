using Microsoft.Xna.Framework;

namespace Match3.Graphics;

/// <summary>
/// Анимация плавного перемещения точки от Start до End за указанное время.
/// </summary>
public class PositionTween {
    public Vector2 Start { get; set; }
    public Vector2 End { get; set; }
    public float Duration { get; set; }
    public float Elapsed { get; private set; }

    public bool IsFinished => Elapsed >= Duration;

    /// <summary>
    /// Текущая интерполированная позиция (Lerp от Start до End).
    /// </summary>
    public Vector2 Current {
        get {
            if (Duration <= 0f) return End;
            float t = MathHelper.Clamp(Elapsed / Duration, 0f, 1f);
            return Vector2.Lerp(Start, End, t);
        }
    }

    public PositionTween(Vector2 start, Vector2 end, float durationSeconds) {
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
