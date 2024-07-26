using System;
using Volo.Abp.Data;

namespace Volo.Abp.BackgroundJobs.Plus;

/// <summary>
/// Represents a background job info that is used to persist jobs.
/// </summary>
public class BackgroundJobPlusInfo : IHasExtraProperties
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundJobPlusInfo"/> class.
    /// </summary>
    public BackgroundJobPlusInfo()
    {
        ExtraProperties = new ExtraPropertyDictionary();
        this.SetDefaultsForExtraProperties();
        Priority = BackgroundJobPriority.Normal;
    }

    public Guid Id { get; set; }

    /// <summary>
    /// Name of the job.
    /// </summary>
    public virtual string JobName { get; set; } = default!;

    /// <summary>
    /// Job arguments as serialized to string.
    /// </summary>
    public virtual string JobArgs { get; set; } = default!;

    /// <summary>
    /// Try count of this job.
    /// A job is re-tried if it fails.
    /// </summary>
    public virtual short TryCount { get; set; }

    /// <summary>
    /// Delay time of this job.
    /// </summary>
    public virtual DateTime DelayTimne { get; set; }

    /// <summary>
    /// Creation time of this job.
    /// </summary>
    public virtual DateTime CreationTime { get; set; }

    /// <summary>
    /// Next try time of this job.
    /// </summary>
    public virtual DateTime NextTryTime { get; set; }

    /// <summary>
    /// Last try time of this job.
    /// </summary>
    public virtual DateTime? LastTryTime { get; set; }

    /// <summary>
    /// This is true if this job is continuously failed and will not be executed again.
    /// </summary>
    public virtual bool IsAbandoned { get; set; }

    /// <summary>
    /// Priority of this job.
    /// </summary>
    public virtual BackgroundJobPriority Priority { get; set; }

    public ExtraPropertyDictionary ExtraProperties { get; protected set; }
}