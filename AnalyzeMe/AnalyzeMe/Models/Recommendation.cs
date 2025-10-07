namespace AnalyzeMe.Models
{
    public enum RecommendationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class Recommendation
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Action { get; set; }
        public RecommendationPriority Priority { get; set; }
        public string? Category { get; set; }
        public double EstimatedImpact { get; set; }
        public bool IsImplemented { get; set; }

        public string PriorityIcon => Priority switch
        {
            RecommendationPriority.Critical => "🔥",
            RecommendationPriority.High => "⚡",
            RecommendationPriority.Medium => "⚠️",
            RecommendationPriority.Low => "💡",
            _ => "•"
        };

        public string PriorityColor => Priority switch
        {
            RecommendationPriority.Critical => "#ff0000",
            RecommendationPriority.High => "#ff00ff",
            RecommendationPriority.Medium => "#ffaa00",
            RecommendationPriority.Low => "#00ffff",
            _ => "#6a7a8a"
        };
    }
}