using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

namespace SDUI.Animation;

public class AnimationEngine
{
    private readonly List<double> animationProgresses;
    private readonly List<Point> animationSources;
    private readonly List<AnimationDirection> animationDirections;
    private readonly List<object[]> animationDatas;

    private const double MIN_VALUE = 0.00;
    private const double MAX_VALUE = 1.00;

    public bool InterruptAnimation { get; set; }
    public double Increment { get; set; }
    public double SecondaryIncrement { get; set; }
    public AnimationType AnimationType { get; set; }
    public bool Singular { get; set; }
    public bool Running { get; private set; }

    public event Action<object> OnAnimationFinished;
    public event Action<object> OnAnimationProgress;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="singular">If true, only one animation is supported. The current animation will be replaced with the new one. If false, a new animation is added to the list.</param>
    public AnimationEngine(bool singular = true)
    {
        const int initialCapacity = 4;
        animationProgresses = new List<double>(initialCapacity);
        animationSources = new List<Point>(initialCapacity);
        animationDirections = new List<AnimationDirection>(initialCapacity);
        animationDatas = new List<object[]>(initialCapacity);

        Increment = 0.03;
        SecondaryIncrement = 0.03;
        AnimationType = AnimationType.Linear;
        InterruptAnimation = true;
        Singular = singular;

        if (Singular)
        {
            animationProgresses.Add(0);
            animationSources.Add(default);
            animationDirections.Add(AnimationDirection.In);
        }

        AnimationEngineProvider.Handle(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AnimationTimerOnTick(object sender, EventArgs eventArgs)
    {
        if (animationProgresses.Count == 0)
        {
            Running = false;
            return;
        }

        var count = animationProgresses.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            var direction = animationDirections[i];
            var progress = animationProgresses[i];

            UpdateProgress(i);

            if (!Singular)
            {
                HandleNonSingularAnimation(i, direction, progress);
            }
            else
            {
                HandleSingularAnimation(i, direction, progress);
            }
        }

        OnAnimationProgress?.Invoke(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void HandleNonSingularAnimation(int index, AnimationDirection direction, double progress)
    {
        switch (direction)
        {
            case AnimationDirection.InOutIn when progress == MAX_VALUE:
                animationDirections[index] = AnimationDirection.InOutOut;
                break;
            case AnimationDirection.InOutRepeatingIn when progress == MIN_VALUE:
                animationDirections[index] = AnimationDirection.InOutRepeatingOut;
                break;
            case AnimationDirection.InOutRepeatingOut when progress == MIN_VALUE:
                animationDirections[index] = AnimationDirection.InOutRepeatingIn;
                break;
            case AnimationDirection.In when progress == MAX_VALUE:
            case AnimationDirection.Out when progress == MIN_VALUE:
            case AnimationDirection.InOutOut when progress == MIN_VALUE:
                RemoveAnimation(index);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void HandleSingularAnimation(int index, AnimationDirection direction, double progress)
    {
        switch (direction)
        {
            case AnimationDirection.InOutIn when progress == MAX_VALUE:
                animationDirections[index] = AnimationDirection.InOutOut;
                break;
            case AnimationDirection.InOutRepeatingIn when progress == MAX_VALUE:
                animationDirections[index] = AnimationDirection.InOutRepeatingOut;
                break;
            case AnimationDirection.InOutRepeatingOut when progress == MIN_VALUE:
                animationDirections[index] = AnimationDirection.InOutRepeatingIn;
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RemoveAnimation(int index)
    {
        animationProgresses.RemoveAt(index);
        animationSources.RemoveAt(index);
        animationDirections.RemoveAt(index);
        animationDatas.RemoveAt(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartNewAnimation(AnimationDirection animationDirection, object[] data = null)
    {
        StartNewAnimation(animationDirection, default, data);
    }

    public void StartNewAnimation(AnimationDirection animationDirection, Point animationSource, object[] data = null)
    {
        if (!IsAnimating() || InterruptAnimation)
        {
            if (Singular && animationDirections.Count > 0)
            {
                animationDirections[0] = animationDirection;
                if (animationSources.Count > 0)
                    animationSources[0] = animationSource;
                if (animationDatas.Count > 0)
                    animationDatas[0] = data ?? Array.Empty<object>();
            }
            else
            {
                animationDirections.Add(animationDirection);
                animationSources.Add(animationSource);
                animationDatas.Add(data ?? Array.Empty<object>());

                if (!(Singular && animationProgresses.Count > 0))
                {
                    animationProgresses.Add(animationDirection switch
                    {
                        AnimationDirection.InOutRepeatingIn or AnimationDirection.InOutIn or AnimationDirection.In => MIN_VALUE,
                        AnimationDirection.InOutRepeatingOut or AnimationDirection.InOutOut or AnimationDirection.Out => MAX_VALUE,
                        _ => throw new ArgumentException("Invalid AnimationDirection")
                    });
                }
            }
        }

        Running = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateProgress(int index)
    {
        switch (animationDirections[index])
        {
            case AnimationDirection.InOutRepeatingIn:
            case AnimationDirection.InOutIn:
            case AnimationDirection.In:
                IncrementProgress(index);
                break;

            case AnimationDirection.InOutRepeatingOut:
            case AnimationDirection.InOutOut:
            case AnimationDirection.Out:
                DecrementProgress(index);
                break;

            default:
                throw new ArgumentException("No AnimationDirection has been set");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void IncrementProgress(int index)
    {
        animationProgresses[index] += Increment;
        if (animationProgresses[index] > MAX_VALUE)
        {
            animationProgresses[index] = MAX_VALUE;
            CheckAnimationCompletion();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DecrementProgress(int index)
    {
        animationProgresses[index] -= (animationDirections[index] == AnimationDirection.InOutOut || 
                                     animationDirections[index] == AnimationDirection.InOutRepeatingOut) 
            ? SecondaryIncrement : Increment;

        if (animationProgresses[index] < MIN_VALUE)
        {
            animationProgresses[index] = MIN_VALUE;
            CheckAnimationCompletion();
        }
    }

    private void CheckAnimationCompletion()
    {
        for (int i = 0; i < GetAnimationCount(); i++)
        {
            var direction = animationDirections[i];
            var progress = animationProgresses[i];

            if (direction == AnimationDirection.InOutIn || 
                direction == AnimationDirection.InOutRepeatingIn || 
                direction == AnimationDirection.InOutRepeatingOut)
                return;

            if ((direction == AnimationDirection.InOutOut && progress != MIN_VALUE) ||
                (direction == AnimationDirection.In && progress != MAX_VALUE) ||
                (direction == AnimationDirection.Out && progress != MIN_VALUE))
                return;
        }

        Running = false;
        OnAnimationFinished?.Invoke(this);
    }

    public bool IsAnimating()
    {
        return Running;
    }

    public double GetProgress()
    {
        if (!Singular)
            throw new Exception("Animation is not set to Singular.");

        if (animationProgresses.Count == 0)
            throw new Exception("Invalid animation");

        return GetProgress(0);
    }

    public double GetProgress(int index)
    {
        if (!(index < GetAnimationCount()))
            throw new IndexOutOfRangeException("Invalid animation index");

        switch (AnimationType)
        {
            case AnimationType.Linear:
                return AnimationLinear.CalculateProgress(animationProgresses[index]);

            case AnimationType.EaseInOut:
                return AnimationEaseInOut.CalculateProgress(animationProgresses[index]);

            case AnimationType.EaseOut:
                return AnimationEaseOut.CalculateProgress(animationProgresses[index]);

            case AnimationType.CustomQuadratic:
                return AnimationCustomQuadratic.CalculateProgress(animationProgresses[index]);

            default:
                throw new NotImplementedException("The given AnimationType is not implemented");
        }
    }

    public Point GetSource(int index)
    {
        if (!(index < GetAnimationCount()))
            throw new IndexOutOfRangeException("Invalid animation index");

        return animationSources[index];
    }

    public Point GetSource()
    {
        if (!Singular)
            throw new Exception("Animation is not set to Singular.");

        if (animationSources.Count == 0)
            throw new Exception("Invalid animation");

        return animationSources[0];
    }

    public AnimationDirection GetDirection()
    {
        if (!Singular)
            throw new Exception("Animation is not set to Singular.");

        if (animationDirections.Count == 0)
            throw new Exception("Invalid animation");

        return animationDirections[0];
    }

    public AnimationDirection GetDirection(int index)
    {
        if (!(index < animationDirections.Count))
            throw new IndexOutOfRangeException("Invalid animation index");

        return animationDirections[index];
    }

    public object[] GetData()
    {
        if (!Singular)
            throw new Exception("Animation is not set to Singular.");

        if (animationDatas.Count == 0)
            throw new Exception("Invalid animation");

        return animationDatas[0];
    }

    public object[] GetData(int index)
    {
        if (!(index < animationDatas.Count))
            throw new IndexOutOfRangeException("Invalid animation index");

        return animationDatas[index];
    }

    public int GetAnimationCount()
    {
        return animationProgresses.Count;
    }

    public void SetProgress(double progress)
    {
        if (!Singular)
            throw new Exception("Animation is not set to Singular.");

        if (animationProgresses.Count == 0)
            throw new Exception("Invalid animation");

        animationProgresses[0] = progress;
    }

    public void SetDirection(AnimationDirection direction)
    {
        if (!Singular)
            throw new Exception("Animation is not set to Singular.");

        if (animationProgresses.Count == 0)
            throw new Exception("Invalid animation");

        animationDirections[0] = direction;
    }

    public void SetData(object[] data)
    {
        if (!Singular)
            throw new Exception("Animation is not set to Singular.");

        if (animationDatas.Count == 0)
            throw new Exception("Invalid animation");

        animationDatas[0] = data;
    }
}