using System;
using System.Linq;
using System.Threading;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR;

using Temporal;
using MoogModule;
using RacistExperiment;
using System.Security.Cryptography;


namespace MeshisExperiment
{
    public class MeshisHandler : ManagedMonoBehaviour
    {
        private MeshisParameters _parameters;
        private MeshisExperiment _experiment;
        private TemporalResponseHandler _input;
        private TemporalSound _sound;
        private StarsGenerator _scene;
        private DebugTabHandler _debugTabHandler;
        private MoogHandler _moogHandler;

        private Temporal2IntervalTrialState _state;
        private DofParameters _whereCameraShouldBe;
        private MeshisTrial _currentTrial;
        private Transform _camera;


        public override void ManagedAwake()
        {
            _parameters = new MeshisParameters { TrialsNumber = 4, StimulusType = TemporalExperimentStimulusType.CombinedCombined };
            _experiment = new MeshisExperiment(_parameters);

            _debugTabHandler = GetComponent<DebugTabHandler>();
            _moogHandler = GetComponent<MoogHandler>();
            _input = GetComponent<TemporalResponseHandler>();
            _sound = GameObject.Find("Audio").GetComponent<TemporalSound>();
            _scene = GameObject.Find("MeshisExperiment").GetComponent<StarsGenerator>();

            _state = Temporal2IntervalTrialState.None;
            _whereCameraShouldBe = _parameters.CameraStartPosition;
            _camera = GameObject.Find("Cameras").transform;
            _currentTrial = null;

            _input.GotAnswer_Up += HandleInput_Up;
            _input.GotAnswer_Left += HandleInput_Aside;
            _input.GotAnswer_Right += HandleInput_Aside;
            _input.GotSignal_Start += HandleInput_Start;

            _debugTabHandler.testBtn1Clicked += (eventObj) => EngageMoog();
            _debugTabHandler.testBtn2Clicked += (eventObj) => StartExperiment();
        }

        public override void ManagedUpdate()
        {
            _camera.position = new Vector3
            {
                z = _whereCameraShouldBe.Surge,
                y = _parameters.CameraStartPosition.Heave,
                x = _whereCameraShouldBe.Sway
            };
        }

        // ..........................................................

        private void EngageMoog()
        {
            _moogHandler.Engage();
        }

        private void DisengageMoog()
        {
            _moogHandler.Disengage();
        }

        private void StartExperiment()
        {
            CanUseUpdateMethod = true;
            StartCoroutine(Loop());
        }

        private IEnumerator Loop()
        {
            Debug.Log("Experiment started");
            _experiment.GenerateTrials();
            yield return MoveMoogToStartPosition();

            while (!_experiment.HasFinished)
            {
                yield return Initializing();
                // here waiting for 'start btn' event
                yield return WaitingToStartSignal();
                yield return FirstInterval();
                yield return InterIntervalPause();
                yield return SecondInterval();
                yield return AnswerPhase();
                yield return PreReturningPause();
                yield return Returning();
                yield return Analyzation();
            }

            _experiment.Save();
            yield return MoveMoogToOriginPosition();
            DisengageMoog();
            Debug.Log("Experiment finished");
        }

        private IEnumerator MoveMoogToStartPosition()
        {
            if (_parameters.StimulusType != TemporalExperimentStimulusType.CombinedCombined)
                yield return null;

            var trajectoryParameters = new MoveByTrajectoryParameters
            {
                StartPoint = new DofParameters { Heave = -0.22f },
                EndPoint = _parameters.MoogStartPosition,
                MovementDuration = TimeSpan.FromSeconds(2),
                TrajectoryType = TrajectoryType.Linear,
                TrajectoryProfile = TrajectoryProfile.CDF,
                DelayHandling = DelayCompensationStrategy.Ignore,
                TrajectoryTypeSettings = new TrajectoryTypeSettings { Linear = new TrajectoryTypeSettings_Linear { } },
                TrajectoryProfileSettings = new TrajectoryProfileSettings { CDF = new TrajectoryProfileSettings_CDF { Sigmas = 3 } }
            };

            _moogHandler.MoveByTrajectory(trajectoryParameters);
            yield return new WaitForSeconds(3);
        }

        private IEnumerator MoveMoogToOriginPosition()
        {
            if (_parameters.StimulusType != TemporalExperimentStimulusType.CombinedCombined)
                yield return null;

            var trajectoryParameters = new MoveByTrajectoryParameters
            {
                StartPoint = _parameters.MoogStartPosition,
                EndPoint = new DofParameters { Heave = -0.22f },
                MovementDuration = TimeSpan.FromSeconds(2),
                TrajectoryType = TrajectoryType.Linear,
                TrajectoryProfile = TrajectoryProfile.CDF,
                DelayHandling = DelayCompensationStrategy.Ignore,
                TrajectoryTypeSettings = new TrajectoryTypeSettings { Linear = new TrajectoryTypeSettings_Linear { } },
                TrajectoryProfileSettings = new TrajectoryProfileSettings { CDF = new TrajectoryProfileSettings_CDF { Sigmas = 3 } }
            };

            _moogHandler.MoveByTrajectory(trajectoryParameters);
            yield return new WaitForSeconds(3);
        }

        private IEnumerator Initializing()
        {
            _state = Temporal2IntervalTrialState.Initializing;
            yield return new WaitForSeconds((float)_parameters.DelayBeforeStartSound.TotalSeconds);

            _sound.PlaySound_Start();
            _state = Temporal2IntervalTrialState.ReadyToStart;
        }

        private IEnumerator WaitingToStartSignal()
        {
            while (_state != Temporal2IntervalTrialState.PreFirstIntervalPause)
                yield return null;

            yield return new WaitForSeconds((float)_parameters.DelayAfterStartSignal.TotalSeconds);
            _currentTrial = _experiment.StartTrial();
            _state = Temporal2IntervalTrialState.FirstInterval;
        }

        private IEnumerator FirstInterval()
        {
            // resolves visual glitch of sudden tp
            _whereCameraShouldBe = new DofParameters { Surge = 0 };
            yield return null;

            _scene.RandomizeStars();
            _scene.Show();

            if (_parameters.StimulusType == TemporalExperimentStimulusType.VestibularVestibular || _parameters.StimulusType == TemporalExperimentStimulusType.CombinedCombined)
            {
                var toPoint = _parameters.MoogStartPosition;

                if (_currentTrial.FirstInterval.Direction == TwoIntervalDirection.Straight)
                    toPoint.Surge += _currentTrial.FirstInterval.Distance;
                else
                    toPoint.Sway += _currentTrial.FirstInterval.Distance;

                var trajectoryParameters = new MoveByTrajectoryParameters
                {
                    StartPoint = _parameters.MoogStartPosition,
                    EndPoint = toPoint,
                    MovementDuration = _parameters.FirstMovementDuration * _parameters.MoogMovementDurationCorrectionFactor,
                    TrajectoryType = TrajectoryType.Linear,
                    TrajectoryProfile = TrajectoryProfile.CDF,
                    DelayHandling = DelayCompensationStrategy.Ignore,
                    TrajectoryTypeSettings = new TrajectoryTypeSettings { Linear = new TrajectoryTypeSettings_Linear { } },
                    TrajectoryProfileSettings = new TrajectoryProfileSettings { CDF = new TrajectoryProfileSettings_CDF { Sigmas = 3 } }
                };

                /*if (_experiment.GetCurrentTrialIndex() == 0)
                    _moogHandler.RecordFeedback(TimeSpan.FromSeconds(10));*/

                _moogHandler.MoveByTrajectory(trajectoryParameters);
                yield return new WaitForSeconds((float)_parameters.DelayBetweenMoogAndVr.TotalSeconds);
            }

            TimeSpan elapsed = TimeSpan.Zero;
            var trajectoryManager = new TrajectoryManager(new MoveByTrajectoryParameters
            {
                StartPoint = new DofParameters { Surge = 0 },
                EndPoint = _currentTrial.FirstInterval.Direction == TwoIntervalDirection.Straight ? new DofParameters { Surge = _currentTrial.FirstInterval.Distance } : new DofParameters { Sway = _currentTrial.FirstInterval.Distance },
                //EndPoint = new DofParameters { Surge = _currentTrial.FirstInterval.Distance },
                MovementDuration = _parameters.FirstMovementDuration,
                TrajectoryType = TrajectoryType.Linear,
                TrajectoryProfile = TrajectoryProfile.CDF,
                DelayHandling = DelayCompensationStrategy.Jump,
                TrajectoryTypeSettings = new TrajectoryTypeSettings { Linear = new TrajectoryTypeSettings_Linear { } },
                TrajectoryProfileSettings = new TrajectoryProfileSettings { CDF = new TrajectoryProfileSettings_CDF { Sigmas = 3 } }
            });

            while (elapsed <= _parameters.FirstMovementDuration)
            {
                elapsed += TimeSpan.FromSeconds(Time.deltaTime);
                float progress = Mathf.Clamp01((float)(elapsed / _parameters.FirstMovementDuration));
                var nextPosition = trajectoryManager.GetNextPosition();

                if (nextPosition == null)
                    break;

                _whereCameraShouldBe = trajectoryManager.GetNextPosition().Value;
                yield return null;
            }

            _state = Temporal2IntervalTrialState.InterIntervalPause;
            _scene.Hide();
        }

        private IEnumerator InterIntervalPause()
        {
            yield return new WaitForSeconds((float)_parameters.PauseBetweenIntervalsDuration.TotalSeconds);
            _state = Temporal2IntervalTrialState.SecondInterval;
        }

        private IEnumerator SecondInterval()
        {
            // resolves visual glitch of sudden tp
            _whereCameraShouldBe = new DofParameters { Surge = 0 };
            yield return null;

            _scene.RandomizeStars();
            _scene.Show();

            if (_parameters.StimulusType == TemporalExperimentStimulusType.VestibularVestibular || _parameters.StimulusType == TemporalExperimentStimulusType.CombinedCombined)
            {
                var toPoint = _parameters.MoogStartPosition;
                if (_currentTrial.FirstInterval.Direction == TwoIntervalDirection.Straight)
                {
                    toPoint.Surge += _currentTrial.FirstInterval.Distance;
                    toPoint.Sway += _currentTrial.SecondInterval.Distance;
                }
                else
                {
                    toPoint.Sway += _currentTrial.FirstInterval.Distance;
                    toPoint.Surge += _currentTrial.SecondInterval.Distance;
                }

                var fromPoint = _parameters.MoogStartPosition;
                if (_currentTrial.FirstInterval.Direction == TwoIntervalDirection.Straight)
                    fromPoint.Surge += _currentTrial.FirstInterval.Distance;
                else
                    fromPoint.Sway += _currentTrial.FirstInterval.Distance;

                var trajectoryParameters = new MoveByTrajectoryParameters
                {
                    StartPoint = fromPoint,
                    EndPoint = toPoint,
                    MovementDuration = _parameters.SecondMovementDuration * _parameters.MoogMovementDurationCorrectionFactor,
                    TrajectoryType = TrajectoryType.Linear,
                    TrajectoryProfile = TrajectoryProfile.CDF,
                    DelayHandling = DelayCompensationStrategy.Ignore,
                    TrajectoryTypeSettings = new TrajectoryTypeSettings { Linear = new TrajectoryTypeSettings_Linear { } },
                    TrajectoryProfileSettings = new TrajectoryProfileSettings { CDF = new TrajectoryProfileSettings_CDF { Sigmas = 3 } }
                };

                _moogHandler.MoveByTrajectory(trajectoryParameters);
                yield return new WaitForSeconds((float)_parameters.DelayBetweenMoogAndVr.TotalSeconds);
            }

            TimeSpan elapsed = TimeSpan.Zero;
            var trajectoryManager = new TrajectoryManager(new MoveByTrajectoryParameters
            {
                StartPoint = new DofParameters { Surge = 0 },
                EndPoint = _currentTrial.SecondInterval.Direction == TwoIntervalDirection.Straight ? new DofParameters { Surge = _currentTrial.SecondInterval.Distance } : new DofParameters { Sway = _currentTrial.SecondInterval.Distance },
                //EndPoint = new DofParameters { Surge = _currentTrial.SecondInterval.Distance },
                MovementDuration = _parameters.SecondMovementDuration,
                TrajectoryType = TrajectoryType.Linear,
                TrajectoryProfile = TrajectoryProfile.CDF,
                DelayHandling = DelayCompensationStrategy.Jump,
                TrajectoryTypeSettings = new TrajectoryTypeSettings { Linear = new TrajectoryTypeSettings_Linear { } },
                TrajectoryProfileSettings = new TrajectoryProfileSettings { CDF = new TrajectoryProfileSettings_CDF { Sigmas = 3 } }
            });

            while (elapsed <= _parameters.SecondMovementDuration)
            {
                elapsed += TimeSpan.FromSeconds(Time.deltaTime);
                float progress = Mathf.Clamp01((float)(elapsed / _parameters.SecondMovementDuration));
                var nextPosition = trajectoryManager.GetNextPosition();

                if (nextPosition == null)
                    break;

                _whereCameraShouldBe = trajectoryManager.GetNextPosition().Value;
                yield return null;
            }

            //Debug.Log(_currentTrial.FirstInterval.Distance);
            //Debug.Log(_currentTrial.FirstInterval.Distance + _currentTrial.SecondInterval.Distance);
            //Debug.Log(_whereCameraShouldBe.Surge);
            //Debug.Log(trajectoryManager.GetDevLog());

            _state = Temporal2IntervalTrialState.AnswerPhase;
            _scene.Hide();
        }

        private IEnumerator AnswerPhase()
        {
            float timeout = (float)_parameters.TimeToAnswer.TotalSeconds;
            float elapsed = 0f;

            while (_currentTrial.ReceivedAnswer == MeshisAnswer.None && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_currentTrial.ReceivedAnswer == MeshisAnswer.None && elapsed >= timeout)
            {
                _experiment.SetParticipantAnswer(MeshisAnswer.Late);
                _sound.PlaySound_AnswerIsLate();
            }

            _state = Temporal2IntervalTrialState.PreReturningPause;
        }

        private IEnumerator PreReturningPause()
        {
            yield return new WaitForSeconds((float)_parameters.DelayAfterAnswerSignal.TotalSeconds);
            _state = Temporal2IntervalTrialState.Returning;
        }

        private IEnumerator Returning()
        {
            /*TimeSpan elapsed = TimeSpan.Zero;
            var trajectoryManager = new TrajectoryManager(new MoveByTrajectoryParameters
            {
                StartPoint = new DofParameters { Surge = _currentTrial.FirstInterval.Distance + _currentTrial.SecondInterval.Distance },
                EndPoint = new DofParameters { Surge = 0 },
                MovementDuration = _parameters.BackwardMovementDuration,
                TrajectoryType = TrajectoryType.Linear,
                TrajectoryProfile = TrajectoryProfile.CDF,
                DelayHandling = DelayCompensationStrategy.Jump,
                TrajectoryTypeSettings = new TrajectoryTypeSettings { Linear = new TrajectoryTypeSettings_Linear { } },
                TrajectoryProfileSettings = new TrajectoryProfileSettings { CDF = new TrajectoryProfileSettings_CDF { Sigmas = 3 } }
            });

            while (elapsed <= _parameters.BackwardMovementDuration)
            {
                elapsed += TimeSpan.FromSeconds(Time.deltaTime);
                float progress = Mathf.Clamp01((float)(elapsed / _parameters.BackwardMovementDuration));
                var nextPosition = trajectoryManager.GetNextPosition();

                if (nextPosition == null)
                    break;

                _whereCameraShouldBe = trajectoryManager.GetNextPosition().Value;
                yield return null;
            }*/

            if (_parameters.StimulusType == TemporalExperimentStimulusType.CombinedCombined)
            {
                var fromPoint = _parameters.MoogStartPosition;
                if (_currentTrial.FirstInterval.Direction == TwoIntervalDirection.Straight)
                {
                    fromPoint.Surge += _currentTrial.FirstInterval.Distance;
                    fromPoint.Sway += _currentTrial.SecondInterval.Distance;
                }
                else
                {
                    fromPoint.Sway += _currentTrial.FirstInterval.Distance;
                    fromPoint.Surge += _currentTrial.SecondInterval.Distance;
                }

                var trajectoryParameters = new MoveByTrajectoryParameters
                {
                    StartPoint = fromPoint,
                    EndPoint = _parameters.MoogStartPosition,
                    MovementDuration = _parameters.BackwardMovementDuration * _parameters.MoogMovementDurationCorrectionFactor,
                    TrajectoryType = TrajectoryType.Linear,
                    TrajectoryProfile = TrajectoryProfile.CDF,
                    DelayHandling = DelayCompensationStrategy.Ignore,
                    TrajectoryTypeSettings = new TrajectoryTypeSettings { Linear = new TrajectoryTypeSettings_Linear { } },
                    TrajectoryProfileSettings = new TrajectoryProfileSettings { CDF = new TrajectoryProfileSettings_CDF { Sigmas = 3 } }
                };

                _moogHandler.MoveByTrajectory(trajectoryParameters);
                yield return new WaitForSeconds((float)_parameters.DelayBetweenMoogAndVr.TotalSeconds);
            }

            yield return new WaitForSeconds((float)_parameters.BackwardMovementDuration.TotalSeconds);
            _state = Temporal2IntervalTrialState.Analyzation;
        }

        private IEnumerator Analyzation()
        {
            _experiment.FinishTrial();
            yield return null;
        }

        // ................

        private void HandleInput_Up()
        {
            if (_state != Temporal2IntervalTrialState.AnswerPhase) return;

            _experiment.SetParticipantAnswer(MeshisAnswer.StraightWasLonger);
            _sound.PlaySound_GotAnswer();
        }

        private void HandleInput_Aside()
        {
            if (_state != Temporal2IntervalTrialState.AnswerPhase) return;

            _experiment.SetParticipantAnswer(MeshisAnswer.AsideWasLonger);
            _sound.PlaySound_GotAnswer();
        }

        private void HandleInput_Start()
        {
            if (_state != Temporal2IntervalTrialState.ReadyToStart) return;

            _state = Temporal2IntervalTrialState.PreFirstIntervalPause;
        }
    }
}