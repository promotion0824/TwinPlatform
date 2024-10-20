import { Stack } from "@mui/material";
import { Button, Select, Textarea } from "@willowinc/ui";
import useApi from "../hooks/useApi";
import { useState } from "react";
import { RuleInstanceBooleanFilter, RuleInstanceReviewStatusLookup } from "./Lookups";
import { RuleInstancePropertiesDto } from "../Rules";

const SkillDeploymentBulkEditor = (props: { deployments: string[], onPropertiesChanged?: () => void }) => {
  const apiclient = useApi();
  const [propDto, _] = useState<RuleInstancePropertiesDto>(new RuleInstancePropertiesDto());
  const [enabled, setEnabled] = useState<string | null>(null);
  const [reviewStatus, setReviewStatus] = useState<string | null>(null);
  const [comment, setComment] = useState<string | null>(null);

  const booleanFilters = RuleInstanceBooleanFilter.GetInvertedBooleanFilter().map(({ value, label }) => ({
    label: `${label}`, value: `${value}`
  }));
  const statusFilters = RuleInstanceReviewStatusLookup.GetStatusFilter().map(({ value, label }) => ({
    label: `${label}`, value: `${value}`
  }));

  const [isSaving, setSaving] = useState(false);
  const submitChanges = async () => {
    setSaving(true);

    propDto.init({
      ...propDto,
      ids: props.deployments,
      disabled: enabled,
      reviewStatus: reviewStatus,
      comment: comment });

    try {
      // Send the DTO via the API client
      await apiclient.updateRuleInstanceProperties(propDto);

      if (props.onPropertiesChanged) {
        props.onPropertiesChanged();
      }
    } catch (error) {
      console.error("Failed to update deployment properties:", error);
    } finally {
      setSaving(false);
    }
  };
  /*Note: Grid will need to be refereshed*/

  const [showAllDeployments, setShowAllDeployments] = useState(false);

  // Determine which list of deployments to display
  const deploymentsToDisplay = showAllDeployments ? props.deployments : props.deployments.slice(0, 10);

  // Function to toggle showing all deployments
  const toggleShowAllDeployments = () => {
    setShowAllDeployments(!showAllDeployments);
  };

  return (
    <Stack spacing={2} p={2}>
      <Select label="Enabled" clearable data={booleanFilters} onChange={(val) => setEnabled(val ? val : null)}/>
      <Select label="Review status" clearable data={statusFilters} onChange={(val) => setReviewStatus(val ? val : null)} />
      <Textarea label="Comment" maxRows={4} onChange={(e) => setComment(e.target.value)}></Textarea>
      <Button kind="primary" loading={isSaving} style={{ minWidth: '80px' }} disabled={props.deployments.length === 0} onClick={() => submitChanges()}>Submit</Button>
      <span>Editing {props.deployments.length} skill deployments:</span>
      <div>
        {deploymentsToDisplay.map((string, index) => (
          <div key={index} style={{ whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
            {string}
          </div>
        ))}
      </div>
      {props.deployments.length > 10 && !showAllDeployments && (
        <Button kind="secondary" onClick={toggleShowAllDeployments}>
          Show all...
        </Button>
      )}
      {props.deployments.length > 10 && showAllDeployments && (
        <Button kind="secondary" onClick={toggleShowAllDeployments}>
          Show less
        </Button>
      )}
    </Stack>
  );
}

export default SkillDeploymentBulkEditor;
