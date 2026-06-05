namespace Match3.Graphics;

/// <summary>
/// Описывает frame-by-frame анимацию: набор кадров и задержку между ними.
/// </summary>
public class Animation {
    public List<TextureRegion> Frames { get; set; }
    public TimeSpan Delay { get; set; }

    public Animation() {
        Frames = new List<TextureRegion>();
        Delay = TimeSpan.FromMilliseconds(100);
    }

    public Animation(List<TextureRegion> frames, TimeSpan delay) {
        Frames = frames;
        Delay = delay;
    }
}
