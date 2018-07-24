﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Add this bar to an object and link it to a bar (possibly the same object the script is on), and you'll be able to resize the bar object based on a current value, located between a min and max value.
	/// See the HealthBar.cs script for a use case
	/// </summary>
	public class MMProgressBar : MonoBehaviour
	{
        public enum FillModes { LocalScale, FillAmount }
        public enum BarDirections { LeftToRight, RightToLeft, UpToDown, DownToUp }

        [Header("General Settings")]
        /// the local scale or fillamount value to reach when the bar is empty 
        public float StartValue = 0f;
        /// the local scale or fillamount value to reach when the bar is full
        public float EndValue = 1f;
        /// the direction this bar moves to
        public BarDirections BarDirection = BarDirections.LeftToRight;
        /// the foreground bar's fill mode
        public FillModes FillMode = FillModes.LocalScale;

        [Header("Foreground Bar Settings")]
		/// whether or not the foreground bar should lerp
		public bool LerpForegroundBar = true;
		/// the speed at which to lerp the foreground bar
		public float LerpForegroundBarSpeed = 15f;

		[Header("Delayed Bar Settings")]
		/// the delay before the delayed bar moves (in seconds)
		public float Delay = 1f;
		/// whether or not the delayed bar's animation should lerp
		public bool LerpDelayedBar = true;
		/// the speed at which to lerp the delayed bar
		public float LerpDelayedBarSpeed = 15f;

		[Header("Bindings")]
		/// optional - the ID of the player associated to this bar
		public string PlayerID;
		/// the delayed bar
		public Transform DelayedBar;
		/// the main, foreground bar
		public Transform ForegroundBar;

		[Header("Bump")]
		/// whether or not the bar should "bump" when changing value
		public bool BumpScaleOnChange = true;
		/// the duration of the bump animation
		public float BumpDuration = 0.2f;
		/// whether or not the bar should flash when bumping
		public bool ChangeColorWhenBumping = true;
		/// the color to apply to the bar when bumping
		public Color BumpColor = Color.white;
		/// the curve to map the bump animation on
		public AnimationCurve BumpAnimationCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        /// the curve to map the bump animation color animation on
        public AnimationCurve BumpColorAnimationCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);
        /// whether or not the bar is bumping right now
        public bool Bumping { get; protected set; }

        [Header("Realtime")]
        /// whether or not this progress bar should update itself every update (if not, you'll have to update it using the UpdateBar method
        public bool AutoUpdating = false;
        /// the current progress of the bar
        [Range(0f,1f)]
        public float BarProgress;

        [InspectorButton("Bump")]
        public bool TestBumpButton;

        protected float _targetFill;
        protected Vector3 _targetLocalScale = Vector3.one;
		protected float _newPercent;
		protected float _lastUpdateTimestamp;
		protected bool _bump = false;
		protected Color _initialColor;
        protected Vector3 _newScale;

        protected Image _foregroundImage;
        protected Image _delayedImage;

		/// <summary>
		/// On start we store our image component
		/// </summary>
		protected virtual void Start()
		{
            if (ForegroundBar != null)
            {
                _foregroundImage = ForegroundBar.GetComponent<Image>();
            }
			if (DelayedBar != null)
            {
                _delayedImage = DelayedBar.GetComponent<Image>();
            }
		}

		/// <summary>
		/// On Update we update our bars
		/// </summary>
		protected virtual void Update()
		{
            AutoUpdate();
			UpdateFrontBar();
			UpdateDelayedBar();
		}

        protected virtual void AutoUpdate()
        {
            if (!AutoUpdating)
            {
                return;
            }

            _newPercent = MMMaths.Remap(BarProgress, 0f, 1f, StartValue, EndValue);
            _targetFill = _newPercent;
            _lastUpdateTimestamp = Time.time;
        }

		/// <summary>
		/// Updates the front bar's scale
		/// </summary>
		protected virtual void UpdateFrontBar()
		{
			if (ForegroundBar != null)
			{
                if (FillMode == FillModes.LocalScale)
                {
                    _targetLocalScale = Vector3.one;
                    switch (BarDirection)
                    {
                        case BarDirections.LeftToRight:
                            _targetLocalScale.x = _targetFill;
                            break;
                        case BarDirections.RightToLeft:
                            _targetLocalScale.x = 1f - _targetFill;
                            break;
                        case BarDirections.DownToUp:
                            _targetLocalScale.y = _targetFill;
                            break;
                        case BarDirections.UpToDown:
                            _targetLocalScale.y = 1f - _targetFill;
                            break;
                    }

                    if (LerpForegroundBar)
                    {
                        _newScale = Vector3.Lerp(ForegroundBar.localScale, _targetLocalScale, Time.deltaTime * LerpForegroundBarSpeed);
                    }
                    else
                    {
                        _newScale = _targetLocalScale;
                    }

                    ForegroundBar.localScale = _newScale;
                }

                if ((FillMode == FillModes.FillAmount) && (_foregroundImage != null))
                {
                    if (LerpDelayedBar)
                    {
                        _foregroundImage.fillAmount = Mathf.Lerp(_foregroundImage.fillAmount, _targetFill, Time.deltaTime * LerpForegroundBarSpeed);
                    }
                    else
                    {
                        _foregroundImage.fillAmount = _targetFill;
                    }
                }
            }
		}

		/// <summary>
		/// Updates the delayed bar's scale
		/// </summary>
		protected virtual void UpdateDelayedBar()
		{
			if (DelayedBar != null)
			{
				if (Time.time - _lastUpdateTimestamp > Delay)
				{
                    if (FillMode == FillModes.LocalScale)
                    {
                        _targetLocalScale = Vector3.one;

                        switch (BarDirection)
                        {
                            case BarDirections.LeftToRight:
                                _targetLocalScale.x = _targetFill;
                                break;
                            case BarDirections.RightToLeft:
                                _targetLocalScale.x = 1f - _targetFill;
                                break;
                            case BarDirections.DownToUp:
                                _targetLocalScale.y = _targetFill;
                                break;
                            case BarDirections.UpToDown:
                                _targetLocalScale.y = 1f - _targetFill;
                                break;
                        }

                        if (LerpDelayedBar)
                        {
                            _newScale = Vector3.Lerp(DelayedBar.localScale, _targetLocalScale, Time.deltaTime * LerpDelayedBarSpeed);
                        }
                        else
                        {
                            _newScale = _targetLocalScale;
                        }

                        DelayedBar.localScale = _newScale;
                    }

                    if ((FillMode == FillModes.FillAmount) && (_delayedImage != null))
                    {
                        if (LerpDelayedBar)
                        {
                            _delayedImage.fillAmount = Mathf.Lerp(_delayedImage.fillAmount, _targetFill, Time.deltaTime * LerpDelayedBarSpeed);
                        }
                        else
                        {
                            _delayedImage.fillAmount = _targetFill;
                        }
                    }
                }
			}
		}

		/// <summary>
		/// Updates the bar's values based on the specified parameters
		/// </summary>
		/// <param name="currentValue">Current value.</param>
		/// <param name="minValue">Minimum value.</param>
		/// <param name="maxValue">Max value.</param>
		public virtual void UpdateBar(float currentValue,float minValue,float maxValue)
		{
			_newPercent = MMMaths.Remap(currentValue, minValue, maxValue, StartValue, EndValue);
			BarProgress = _newPercent;
			_targetFill = _newPercent;
			_lastUpdateTimestamp = Time.time;
		}

		/// <summary>
		/// Triggers a camera bump
		/// </summary>
		public virtual void Bump()
		{
			if (!BumpScaleOnChange)
			{
				return;
			}
			if (this.gameObject.activeInHierarchy)
			{
				StartCoroutine(BumpCoroutine());
			}
		}

		/// <summary>
		/// A coroutine that (usually quickly) changes the scale of the bar 
		/// </summary>
		/// <returns>The coroutine.</returns>
		protected virtual IEnumerator BumpCoroutine()
		{

			float journey = 0f;
			Bumping = true;
			if (_foregroundImage != null)
			{
				_initialColor = _foregroundImage.color;
			}

			while (journey <= BumpDuration)
			{
				journey = journey + Time.deltaTime;
				float percent = Mathf.Clamp01(journey / BumpDuration);
                float curvePercent = BumpAnimationCurve.Evaluate(percent);
                float colorCurvePercent = BumpColorAnimationCurve.Evaluate(percent);
                this.transform.localScale = curvePercent * Vector3.one;

				if (ChangeColorWhenBumping && (_foregroundImage != null))
				{
					_foregroundImage.color = Color.Lerp(_initialColor, BumpColor, colorCurvePercent);
				}

				yield return null;
			}
            _foregroundImage.color = _initialColor;
            Bumping = false;
			yield break;

		}
	}
}