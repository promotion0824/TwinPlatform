import { ADTActionRequired, ADTActionStatus, CalculatedPointSource, CumulativeType, ReviewStatus, RuleInstanceStatus, UnitOutputType } from '../Rules';

export class RuleInstanceStatusLookup {
  static GetStatusFilter() {
    var filterArray = new Array();

    Object.entries(RuleInstanceStatusLookup.Status).forEach(([key, valueObject]) => {
      filterArray.push({ value: valueObject.id, label: valueObject.name });
    });

    return filterArray;
  }

  static getStatusString(status: RuleInstanceStatus) {
    let result = "";

    RuleInstanceStatusLookup.getStatuses(status).forEach((v) => {
      result += v.name + ", ";
    });

    if (result.length > 0) {
      result = result.substring(0, result.length - 2);
    }

    return result;
  }


  static getStatuses(status: RuleInstanceStatus, returnValid?: boolean) {
    const result = [];

    if (status & RuleInstanceStatus._1 && (returnValid === undefined || returnValid === true)) {
      result.push(RuleInstanceStatusLookup.Status[RuleInstanceStatus._1]);
    }

    if (status & RuleInstanceStatus._2) {
      result.push(RuleInstanceStatusLookup.Status[RuleInstanceStatus._2]);
    }

    if (status & RuleInstanceStatus._4) {
      result.push(RuleInstanceStatusLookup.Status[RuleInstanceStatus._4]);
    }

    if (status & RuleInstanceStatus._8) {
      result.push(RuleInstanceStatusLookup.Status[RuleInstanceStatus._8]);
    }

    if (status & RuleInstanceStatus._16) {
      result.push(RuleInstanceStatusLookup.Status[RuleInstanceStatus._16]);
    }

    if (status & RuleInstanceStatus._32) {
      result.push(RuleInstanceStatusLookup.Status[RuleInstanceStatus._32]);
    }

    return result;
  }

  static Status =
    {
      1: {
        id: "Valid",
        name: "Valid",
        description: "Skill instance is valid."
      },
      2: {
        id: "BindingFailed",
        name: "Binding Failed",
        description: "This instance could not be bound to the Willow Twin Graph. Check the available bindings for the skill instance and adjust the expression as necessary using OPTION expressions."
      },
      4: {
        id: "NonCommissioned",
        name: "Non-commissioned/No capabilities",
        description: "This instance could not be bound to the Willow Twin Graph. Fix the twin itself if it is missing capabilities that it should have."
      },
      8: {
        id: "FilterFailed",
        name: "Invalid Filter",
        description: "This instance has failed one or more of the rule's filtering criteria."
      },
      16: {
        id: "FilterApplied",
        name: "Won't execute due to Filter",
        description: "This instance has one or more filters applied from the rule's filtering criteria."
      },
      32: {
        id: "ArrayUnexpected",
        name: "Contains Array results",
        description: "This instance has an expression expecting a single value result but is an array. Wrap the expression in an Aggregation function to get a single result."
      }
    };
}

export class RuleInstanceReviewStatusLookup {
  static GetStatusFilter() {
    var filterArray = new Array();

    RuleInstanceReviewStatusLookup.Status.forEach((valueObject) => {
      filterArray.push({ value: valueObject.id, label: valueObject.name });
    });

    return filterArray;
  }

  static getStatusString(status: ReviewStatus) {
    return RuleInstanceReviewStatusLookup.Status.find((v) =>  v.id == status)?.name;
  }

  static Status = [{
    id: ReviewStatus._0,
    name: "Not Reviewed",
    description: "Not Reviewed"
  },
  {
    id: ReviewStatus._1,
    name: "In Progress",
    description: "In Progress"
  },
  {
    id: ReviewStatus._2,
    name: "Complete",
    description: "Complete"
  }];
}

export class RuleInstanceBooleanFilter {
  static GetInvertedBooleanFilter() {
    var filterArray = new Array();

    filterArray.push({ value: true, label: 'false' });
    filterArray.push({ value: false, label: 'true' });

    return filterArray;
  }
}

export class CumulativeTypeLookup {
  static GetCumulativeTypeFilter() {
    var filterArray = new Array();

    Object.entries(CumulativeTypeLookup.Types).forEach(([key, valueObject]) => {
      filterArray.push({ key: key, name: valueObject.name, description: valueObject.description });
    });

    return filterArray;
  }

  static GetTypeName(cumulativeType: CumulativeType) {
    let result = "";

    result = this.Types[cumulativeType].name;

    return result;
  }

  static GetDisplayString(cumulativeType: CumulativeType) {
    let result = "";

    result = this.Types[cumulativeType].displayString;

    return result;
  }

  static Types =
    {
      0: {
        name: "Normal Expression",
        description: "The expression used to define the logic and criteria for the rule.",
        displayString: "normal expression"
      },
      1: {
        name: "Accumulated Variable",
        description: "The cumulative sum of values that have been added together over time.",
        displayString: "accumulated variable"
      },
      2: {
        name: "Time-weighted Sum (sec)",
        description: "The cumulative sum of values at each time point, multiplied by the corresponding time value in seconds.",
        displayString: "time-weighted sum in seconds"
      },
      3: {
        name: "Time-weighted Sum (min)",
        description: "The cumulative sum of values at each time point, multiplied by the corresponding time value in minutes.",
        displayString: "time-weighted sum in minutes"
      },
      4: {
        name: "Time-weighted Sum (hrs)",
        description: "The cumulative sum of values at each time point, multiplied by the corresponding time value in hours.",
        displayString: "time-weighted sum in hours"
      }
    };
}

export class CalculatedPointSourceLookup {
  static getSourceString(source: CalculatedPointSource) {
    return CalculatedPointSourceLookup.Items[source];
  }

  static GetSourceFilter() {
    var filterArray = new Array();

    Object.entries(CalculatedPointSourceLookup.Items).forEach(([key, value]) => {
      filterArray.push({ value: key, label: value });
    });

    return filterArray;
  }

  static Items =
    {
      0: "ADT",
      1: "Rules Engine"
    };
}

export class CalculatedPointTypeLookup {
  static getTypeString(type: UnitOutputType) {
    return CalculatedPointTypeLookup.Items[type];
  }

  static GetTypeFilter() {
    var filterArray = new Array();

    Object.entries(CalculatedPointTypeLookup.Items).forEach(([key, value]) => {
      filterArray.push({ value: key, label: value });
    });

    return filterArray;
  }

  static Items =
    {
      0: "Undefined",
      1: "Analog",
      2: "Binary"
    };
}

export class ADTActionRequiredLookup {
  static getActionString(action: ADTActionRequired) {
    return ADTActionRequiredLookup.Items[action];
  }

  static GetActionFilter() {
    var filterArray = new Array();

    Object.entries(ADTActionRequiredLookup.Items).forEach(([key, value]) => {
      filterArray.push({ value: key, label: value });
    });

    return filterArray;
  }

  static Items =
    {
      0: "None",
      1: "Upsert",
      2: "Delete"
    };
}

export class ADTActionStatusLookup {
  static getStatusString(status: ADTActionStatus) {
    return ADTActionStatusLookup.Items[status];
  }

  static GetStatusFilter() {
    var filterArray = new Array();

    Object.entries(ADTActionStatusLookup.Items).forEach(([key, value]) => {
      filterArray.push({ value: key, label: value });
    });

    return filterArray;
  }

  static Items =
    {
      0: "Twin Available",
      1: "No Twin",
      2: "Failed"
    };
}
