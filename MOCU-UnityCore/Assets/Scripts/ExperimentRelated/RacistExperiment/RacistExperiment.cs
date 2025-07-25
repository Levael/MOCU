using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using System;

using DaemonsRelated;
using UnityEngine;


namespace RacistExperiment
{
    public class RacistExperiment
    {
        private List<RacistTrial> _trials;
        private RacistParameters _config;
        private int _multiplierIndex;
        private float _multiplier;
        private int _currentTrialIndex;

        public RacistExperiment(RacistParameters config)
        {
            _config = config;
            _trials = new();
            _multiplierIndex = _config.Multipliers.Count - 1;
            _multiplier = _config.Multipliers[_multiplierIndex];
            _currentTrialIndex = 0;
        }

        public string GetTrialsData()
        {
            return JsonHelper.SerializeJson(_trials);
        }

        public void GenerateTrials()
        {
            int quarter = (int)Math.Floor(_config.TrialsNumber * 0.25);

            _trials.Clear();

            AddTrials(TwoIntervalDistanceType.Reference, TwoIntervalPersonColor.Black, quarter);
            AddTrials(TwoIntervalDistanceType.Reference, TwoIntervalPersonColor.White, quarter);
            AddTrials(TwoIntervalDistanceType.Test, TwoIntervalPersonColor.Black, quarter);
            AddTrials(TwoIntervalDistanceType.Test, TwoIntervalPersonColor.White, quarter);

            // Random shuffle of all trials
            Shuffle(_trials);

            // First trial has easiest task. All subsequent ones will be calculated only after receiving the response
            SetTrialAdditionalData(trialData: _trials[0]);
        }

        public void PrepareNextTrial(bool answerWasCorrect)
        {
            if (answerWasCorrect)
            {
                if (UnityEngine.Random.value < _config.ChanceToMakeTaskHarder)
                    MakeTaskHarder();
            }
            else
            {
                if (UnityEngine.Random.value < _config.ChanceToMakeTaskEasier)
                    MakeTaskEasier();
            }

            _currentTrialIndex++;
            SetTrialAdditionalData(_trials[_currentTrialIndex]);
        }

        // .....................

        private void AddTrials(TwoIntervalDistanceType type, TwoIntervalPersonColor color, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var first = new TwoIntervalExperimentHalf
                {
                    DistanceType = type,
                    PersonColor = color
                };

                var second = new TwoIntervalExperimentHalf
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
            _multiplierIndex = Mathf.Clamp(_multiplierIndex - 1, 0, _config.Multipliers.Count - 1);
            _multiplier = _config.Multipliers[_multiplierIndex];
        }

        private void MakeTaskEasier()
        {
            _multiplierIndex = Mathf.Clamp(_multiplierIndex + 1, 0, _config.Multipliers.Count - 1);
            _multiplier = _config.Multipliers[_multiplierIndex];
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

        private void SetTrialAdditionalData(RacistTrial trialData)
        {
            var testDistance = _config.ReferenceDistance * _multiplier;

            if (trialData.FirstInterval.DistanceType == TwoIntervalDistanceType.Reference)
                trialData.SecondInterval.Distance = testDistance;
            else
                trialData.FirstInterval.Distance = testDistance;
        }
    }
}