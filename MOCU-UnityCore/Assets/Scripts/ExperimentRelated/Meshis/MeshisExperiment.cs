/*using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using System;

using DaemonsRelated;
using UnityEngine;

using RacistExperiment;


namespace MeshisExperiment
{
    public class MeshisExperiment
    {
        private List<RacistTrial> _trials;
        private MeshisParameters _config;
        private int _multiplierIndex;
        private float _multiplier;
        private int _currentTrialIndex;

        public DateTime StartedAt;
        public DateTime FinishedAt;
        public bool HasFinished;

        public MeshisExperiment(MeshisParameters config)
        {
            _config = config;
            _trials = new();
            _multiplierIndex = _config.Multipliers.Count - 1;
            _multiplier = _config.Multipliers[_multiplierIndex];
            _currentTrialIndex = 0;
            HasFinished = false;
        }

        public string GetTrialsData()
        {
            return JsonHelper.SerializeJson(_trials);
        }

        public void GenerateTrials()
        {
            if (_config.TrialsNumber < 4)
                throw new Exception("Number of trials must be at least 4");

            int quarter = (int)Math.Floor(_config.TrialsNumber * 0.25);

            _trials.Clear();

            AddTrials(TwoIntervalDistanceType.Reference, TwoIntervalDirection.Forward, quarter);
            AddTrials(TwoIntervalDistanceType.Reference, TwoIntervalDirection.Aside, quarter);
            AddTrials(TwoIntervalDistanceType.Test, TwoIntervalDirection.Forward, quarter);
            AddTrials(TwoIntervalDistanceType.Test, TwoIntervalDirection.Aside, quarter);

            // Random shuffle of all trials
            Shuffle(_trials);

            // First trial has easiest task. All subsequent ones will be calculated only after receiving the response
            SetTrialAdditionalData();
        }

        public void PrepareNextTrial()
        {
            var currentTrial = _trials[_currentTrialIndex];
            var answerWasCorrect = currentTrial.ReceivedAnswer == currentTrial.CorrectAnswer;

            if (currentTrial.ReceivedAnswer == RacistAnswer.Late)
                LeaveSameDifficulty();
            else if(answerWasCorrect)
                MakeTaskHarder();
            else
                MakeTaskEasier();

            _currentTrialIndex += 1;
            SetTrialAdditionalData();
        }

        public void SetParticipantAnswer(RacistAnswer answer)
        {
            _trials[_currentTrialIndex].ReceivedAnswer = answer;
        }

        public RacistTrial StartTrial()
        {
            var trial = _trials[_currentTrialIndex];
            trial.StartedAt = DateTime.UtcNow;
            Debug.Log($"_multiplier - {_multiplier}");
            return trial;
        }

        public void FinishTrial()
        {
            _trials[_currentTrialIndex].FinishedAt = DateTime.UtcNow;

            if (_currentTrialIndex >= _trials.Count - 1)
            {
                HasFinished = true;
                return;
            }

            PrepareNextTrial();
        }

        public void Save()
        {
            new RacistSavedData { Trials = _trials, Parameters = _config }.Save();
        }

        // .....................

        private void AddTrials(TwoIntervalDistanceType type, TwoIntervalDirection direction, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var first = new TwoIntervalRacistExperimentHalf
                {
                    DistanceType = type,
                    PersonColor = color
                };

                var second = new TwoIntervalRacistExperimentHalf
                {
                    DistanceType = GetComplementaryDistanceType(type),
                    PersonColor = GetComplementaryColor(color)
                };

                _trials.Add(new RacistTrial
                {
                    FirstInterval = first,
                    SecondInterval = second
                });
            }
        }

        private void MakeTaskHarder()
        {
            if (UnityEngine.Random.value > _config.ChanceToMakeTaskHarder)
                return;

            _multiplierIndex = Mathf.Clamp(_multiplierIndex - 1, 0, _config.Multipliers.Count - 1);
            _multiplier = _config.Multipliers[_multiplierIndex];
        }

        private void MakeTaskEasier()
        {
            if (UnityEngine.Random.value > _config.ChanceToMakeTaskEasier)
                return;

            _multiplierIndex = Mathf.Clamp(_multiplierIndex + 1, 0, _config.Multipliers.Count - 1);
            _multiplier = _config.Multipliers[_multiplierIndex];
        }

        private void LeaveSameDifficulty()
        {
            // nothing
        }


        // ...........................................

        private void Shuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private TwoIntervalPersonColor GetComplementaryColor(TwoIntervalPersonColor color)
        {
            return color == TwoIntervalPersonColor.Black ? TwoIntervalPersonColor.White : TwoIntervalPersonColor.Black;
        }

        private TwoIntervalDistanceType GetComplementaryDistanceType(TwoIntervalDistanceType type)
        {
            return type == TwoIntervalDistanceType.Reference ? TwoIntervalDistanceType.Test : TwoIntervalDistanceType.Reference;
        }

        private void SetTrialAdditionalData()
        {
            var trialData = _trials[_currentTrialIndex];
            var testDistance = _config.ReferenceDistance * _multiplier;
            //Debug.Log($"_config.ReferenceDistance: {_config.ReferenceDistance}, _multiplier: {_multiplier}");

            if (trialData.FirstInterval.DistanceType == TwoIntervalDistanceType.Reference)
            {
                trialData.FirstInterval.Distance = _config.ReferenceDistance;
                trialData.SecondInterval.Distance = testDistance;
            }
            else
            {
                trialData.FirstInterval.Distance = testDistance;
                trialData.SecondInterval.Distance = _config.ReferenceDistance;
            }

            trialData.CorrectAnswer =
                trialData.FirstInterval.Distance > trialData.SecondInterval.Distance ?
                RacistAnswer.FirstWasLonger :
                RacistAnswer.SecondWasLonger;
        }
    }
}*/