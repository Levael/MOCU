using System;
using UnityEngine;
using System.Collections;

using MoogModule;
using UnityEngine.InputSystem.XR;
using Unity.VisualScripting;
using System.Threading;
using UnityEditor;
using System.Linq;


namespace RacistExperiment
{
    public class RacistHandler : MonoBehaviour
    {
        private RacistParameters _parameters;
        private RacistExperiment _experiment;
        private RacistResponseHandler _input;
        private RacistSound _sound;
        private RacistScene _scene;

        private RacistTrialState _state;
        private DofParameters _cameraStartPosition;
        private DofParameters _moogStartPosition;
        private DofParameters _whereCameraShouldBe;
        private RacistTrial _currentTrial;
        private Transform _camera;
        private TrajectoryManager _trajectoryManager;


        private void Awake()
        {
            _parameters = new RacistParameters { TrialsNumber = 4, StimulusType = RacistStimulusType.VisualVisual };
            _experiment = new RacistExperiment(_parameters);
            _input = GetComponent<RacistResponseHandler>();
            _sound = GetComponent<RacistSound>();
            _scene = GetComponent<RacistScene>();

            _state = RacistTrialState.None;
            _cameraStartPosition = new DofParameters { Surge = 0, Heave = 1.7f, Sway = 0 };
            _moogStartPosition = new DofParameters { Surge = -0.12f, Heave = -0.22f, Sway = 0 };
            _whereCameraShouldBe = _cameraStartPosition;
            _camera = GameObject.Find("VR").transform;
            _currentTrial = null;

            _input.GotAnswer_Up += HandleInput_Up;
            _input.GotAnswer_Down += HandleInput_Down;
            _input.GotSignal_Start += HandleInput_Start;
        }

        private void Start()
        {
            StartCoroutine(Loop());
        }

        private void Update()
        {
            _camera.position = new Vector3 { z = _whereCameraShouldBe.Surge, y = _cameraStartPosition.Heave, x = _cameraStartPosition.Sway };
            //_camera.position = new Vector3 { z = _whereCameraShouldBe.Surge, y = _whereCameraShouldBe.Heave, x = _whereCameraShouldBe.Sway };
        }

        private IEnumerator Loop()
        {
            Debug.Log("Experiment started");
            _experiment.GenerateTrials();

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
            Debug.Log("Experiment finished");
        }

        private IEnumerator Initializing()
        {
            _state = RacistTrialState.Initializing;
            yield return new WaitForSeconds((float)_parameters.DelayBeforeStartSound.TotalSeconds);

            _sound.PlaySound_Start();
            _state = RacistTrialState.ReadyToStart;
        }

        private IEnumerator WaitingToStartSignal()
        {
            while (_state != RacistTrialState.PreFirstIntervalPause)
                yield return null;

            yield return new WaitForSeconds((float)_parameters.DelayAfterStartSignal.TotalSeconds);
            _currentTrial = _experiment.StartTrial();
            _state = RacistTrialState.FirstInterval;
        }

        private IEnumerator FirstInterval()
        {
            if (_currentTrial.FirstInterval.PersonColor == TwoIntervalPersonColor.White)
                _scene.ShowWhiteModel();
            else if (_currentTrial.FirstInterval.PersonColor == TwoIntervalPersonColor.Black)
                _scene.ShowBlackModel();
            else
                Debug.Log("You shouldn't see that message");

            TimeSpan elapsed = TimeSpan.Zero;
            var trajectoryManager = new TrajectoryManager(new MoveByTrajectoryParameters
            {
                StartPoint = new DofParameters { Surge = 0 },
                EndPoint = new DofParameters { Surge = _currentTrial.FirstInterval.Distance },
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

            _state = RacistTrialState.InterIntervalPause;
            _scene.HideModel();
        }

        private IEnumerator InterIntervalPause()
        {
            yield return new WaitForSeconds((float)_parameters.PauseBetweenIntervalsDuration.TotalSeconds);
            _state = RacistTrialState.SecondInterval;
        }

        private IEnumerator SecondInterval()
        {
            if (_currentTrial.SecondInterval.PersonColor == TwoIntervalPersonColor.White)
                _scene.ShowWhiteModel();
            else if (_currentTrial.SecondInterval.PersonColor == TwoIntervalPersonColor.Black)
                _scene.ShowBlackModel();
            else
                Debug.Log("You shouldn't see that message");

            TimeSpan elapsed = TimeSpan.Zero;
            var trajectoryManager = new TrajectoryManager(new MoveByTrajectoryParameters
            {
                StartPoint = new DofParameters { Surge = 0 },
                EndPoint = new DofParameters { Surge = _currentTrial.SecondInterval.Distance },
                MovementDuration = _parameters.SecondMovementDuration,
                TrajectoryType = TrajectoryType.Linear,
                TrajectoryProfile = TrajectoryProfile.CDF,
                DelayHandling = DelayCompensationStrategy.Jump,
                TrajectoryTypeSettings = new TrajectoryTypeSettings { Linear = new TrajectoryTypeSettings_Linear { } },
                TrajectoryProfileSettings = new TrajectoryProfileSettings { CDF = new TrajectoryProfileSettings_CDF { Sigmas = 3 } }
            });

            //Debug.Log(_whereCameraShouldBe.Surge);

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

            _state = RacistTrialState.AnswerPhase;
            _scene.HideModel();
        }

        private IEnumerator AnswerPhase()
        {
            float timeout = (float)_parameters.TimeToAnswer.TotalSeconds;
            float elapsed = 0f;

            while (_currentTrial.ReceivedAnswer == RacistAnswer.None && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_currentTrial.ReceivedAnswer == RacistAnswer.None && elapsed >= timeout)
            {
                _experiment.SetParticipantAnswer(RacistAnswer.Late);
                _sound.PlaySound_AnswerIsLate();
            }

            _state = RacistTrialState.PreReturningPause;
        }

        private IEnumerator PreReturningPause()
        {
            yield return new WaitForSeconds((float)_parameters.DelayAfterAnswerSignal.TotalSeconds);
            _state = RacistTrialState.Returning;
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

            yield return new WaitForSeconds((float)_parameters.BackwardMovementDuration.TotalSeconds);
            _state = RacistTrialState.Analyzation;
        }

        private IEnumerator Analyzation()
        {
            _experiment.FinishTrial();
            yield return null;
        }

        // ................

        private void HandleInput_Up()
        {
            if (_state != RacistTrialState.AnswerPhase) return;

            _experiment.SetParticipantAnswer(RacistAnswer.SecondWasLonger);
            _sound.PlaySound_GotAnswer();
        }

        private void HandleInput_Down()
        {
            if (_state != RacistTrialState.AnswerPhase) return;

            _experiment.SetParticipantAnswer(RacistAnswer.FirstWasLonger);
            _sound.PlaySound_GotAnswer();
        }

        private void HandleInput_Start()
        {
            if (_state != RacistTrialState.ReadyToStart) return;

            _state = RacistTrialState.PreFirstIntervalPause;
        }
    }
}