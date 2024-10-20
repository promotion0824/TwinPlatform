import { Grid, Stack } from '@mui/material';
import FlexTitle from '../components/FlexPageTitle';
import SkillDeployments from '../components/grids/SkillDeployments';
import StyledLink from '../components/styled/StyledLink';

const RuleInstancesPage = () => {
  return (
    <Stack spacing={2}>
      <Grid container>
        <Grid item xs={12} md={4}>
          <FlexTitle>
            <StyledLink to={"/rules"}>Skills</StyledLink>
            Skill Deployment
          </FlexTitle>
        </Grid>
      </Grid>
      <SkillDeployments pageId='SkillDeployment' ruleId="all" showRuleColumnsByDefault={true} />
    </Stack>
  );
}

export default RuleInstancesPage;
