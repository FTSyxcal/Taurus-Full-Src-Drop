
using System;
namespace Taurus.DiscordRPC.Helper;
internal class BackoffDelay
{
  private int _current;
  private int _fails;
  public int Maximum { get; private set; }
  public int Minimum { get; private set; }
  public int Current => this._current;
  public int Fails => this._fails;
  public Random Random { get; set; }
  private BackoffDelay()
  {
  }
  public BackoffDelay(int min, int max)
    : this(min, max, new Random())
  {
  }
  public BackoffDelay(int min, int max, Random random)
  {
    this.Minimum = min;
    this.Maximum = max;
    this._current = min;
    this._fails = 0;
    this.Random = random;
  }
  public void Reset()
  {
    this._fails = 0;
    this._current = this.Minimum;
  }
  public int NextDelay()
  {
    ++this._fails;
    this._current = (int) Math.Floor((double) (this.Maximum - this.Minimum) / 100.0 * (double) this._fails) + this.Minimum;
    return Math.Min(Math.Max(this._current, this.Minimum), this.Maximum);
  }
}
