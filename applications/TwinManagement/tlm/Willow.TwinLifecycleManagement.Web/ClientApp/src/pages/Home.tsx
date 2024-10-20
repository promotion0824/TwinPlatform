import { Link } from '@mui/material';
import { UserInfo } from '../components/UserInfo';
import { endpoints } from '../config';

const HomePage = () => {
  return (
    <div>
      <h1>
        <UserInfo />
      </h1>
      <br />
      <p>Twin Lifecycle Management is here to help you mange your digital twin.</p>
      <p>
        See <Link href={endpoints.userGuideLink}>User Guide</Link> for more information on how to use this tool.
      </p>
      <p>
        Reach out to <Link href={endpoints.supportLink}>support@willowinc.com</Link> in case of any issues.
      </p>
      <p>
        Thank you,
        <br />
        TLM Team
      </p>
    </div>
  );
};

export default HomePage;
